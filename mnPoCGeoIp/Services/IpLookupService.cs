using mnPoCGeoIp.Extensions;
using mnPoCGeoIp.DataBase;
using mnPoCGeoIp.Controllers;

namespace mnPoCGeoIp.Services
{
    // #####################################################################################################################################
    /// <summary>
    /// Interface for the single lookup case
    /// </summary>
    public interface IIpLookupService
    {
        /// <summary>
        /// Looks up the IP address information from the remote geo-ip service
        /// </summary>
        /// <param name="ip_addresses"></param>
        /// <returns></returns>
        Task<IEnumerable<Models.ApiRes_IPAddressLookupData>> StoreIpAddrAndWriteResultsAsync(IEnumerable<string> ip_addresses);

        /// <summary>
        /// Looks up the IP address information from the remote geo-ip service and resolves the result
        /// </summary>
        /// <param name="ip_list_data">IP data structure</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        Task<Models.ApiRes_IPAddressLookupData> LookupIpAddressAsync(Models.geoip_lookup_ip ip_list_data);
    }
    // #####################################################################################################################################
    /// <summary>
    /// Interface for the batch mode lookup case
    /// </summary>
    public interface IIpLookupServiceBatch
    {
        /// <summary>
        /// Does the look-up and stores the results in the database
        /// </summary>
        /// <param name="batch_id"></param>
        /// <param name="ip_list_data"></param>
        /// <returns></returns>
        Task<IEnumerable<Models.ApiRes_IPAddressLookupData>> LookupAndStoreResultsAsync(long batch_id, IEnumerable<Models.geoip_lookup_ip> ip_list_data);
    }
    // #####################################################################################################################################
    public class IpLookupService : IIpLookupService, IIpLookupServiceBatch
    {
        private readonly IWebClientService _webClientService;
        private readonly IIpAddressRepository _dbIpRepo;
        private readonly ILogger<IPAddressController> _logger;
        private readonly KeyValuePair<string, string> _authData;

        // ---------------------------------------------------------------------------------------------------------------------
        public IpLookupService(
            IWebClientService webClientService,
            ILogger<IPAddressController> logger,
            IIpAddressRepository dbIpAddrRepo)
        {
            _webClientService = webClientService;
            _dbIpRepo = dbIpAddrRepo;
            _logger = logger;

            _authData = new KeyValuePair<string, string>(
                Appsettings.Current.provider_geoip_auth_keyname,
                Appsettings.Current.provider_geoip_access_token);
        }
        // ---------------------------------------------------------------------------------------------------------------------
        public async Task<IEnumerable<Models.ApiRes_IPAddressLookupData>> LookupAndStoreResultsAsync(long batch_id, IEnumerable<Models.geoip_lookup_ip> ip_list_data)
        {
            var results = new List<Models.ApiRes_IPAddressLookupData>();

            // Update the batch start time
            await _dbIpRepo.UpdateBatchStartOrEndAsync(batch_id, Models.BATCH_UPDATE_TYPES.STARTED);
            bool addDelay = ip_list_data.Count() > 1;

            foreach (var ip_data in ip_list_data)
            {
                // Lookup each IP address
                var lookupRes = await this.LookupIpAddressAsync(ip_data);

                // Add a delay here just for the PoC, so the sequential processing be more observable
                if (addDelay)
                    Task.Delay(TimeSpan.FromSeconds(2)).Wait();

                results.Add(lookupRes);

                // Save to the database each fetched data
                await _dbIpRepo.SaveFetchedDataAsync(batch_id, lookupRes);
            }

            // Finally, update the batch end time
            await _dbIpRepo.UpdateBatchStartOrEndAsync(batch_id, Models.BATCH_UPDATE_TYPES.COMPLETED);

            return results;
        }
        // ---------------------------------------------------------------------------------------------------------------------
        public async Task<IEnumerable<Models.ApiRes_IPAddressLookupData>> StoreIpAddrAndWriteResultsAsync(IEnumerable<string> ip_addresses)
        {
            // Store the request to the database and resolve the batch id
            var storeRes = await _dbIpRepo.StoreIpAddressesAsync(ip_addresses);

            return await this.LookupAndStoreResultsAsync(storeRes.batch_id, storeRes.ip_addresses);
        }
        // ---------------------------------------------------------------------------------------------------------------------
        public async Task<Models.ApiRes_IPAddressLookupData> LookupIpAddressAsync(Models.geoip_lookup_ip ip_list_data)
        {
            _logger.LogInformation("Start lookup IP [{ip}]", ip_list_data.ip_address);

            // Construct the final URL
            string fullUrl = Appsettings.Current.provider_geoip_lookup_url + ip_list_data.ip_address;
            
            try
            {
                // Make the call to the remote service
                var node = await _webClientService.GetJsonDataAsync(fullUrl, _authData);

                // Parse the result
                DateTimeOffset executed_at = DateTimeOffset.UtcNow;
                var data = node["data"] ?? throw new NullReferenceException("data root node does not exist");
                var location = data?["location"] ?? throw new NullReferenceException("location data does not exist");
                var country = location?["country"] ?? throw new NullReferenceException("country data does not exist");
                var timezone = data["timezone"] ?? throw new NullReferenceException("timezone data does not exist");

                var lookup_result = new Models.ApiRes_IPAddressLookupData
                {
                    ip_address = ip_list_data.ip_address,
                    country_iso3_code = country["alpha3"].GetOrThrow<string>(),
                    country_name = country["name"].GetOrThrow<string>(),
                    timezone_id = timezone["id"].GetOrThrow<string>(),
                    lat = location["latitude"].GetOrThrow<decimal>(),
                    lon = location["longitude"].GetOrThrow<decimal>(),
                    ip_id = ip_list_data.exec_order,
                    executed_at = executed_at,
                };

                _logger.LogInformation("Complete lookup IP [{ip}], country: {country}", ip_list_data.ip_address, lookup_result.country_name);

                return lookup_result;
            }
            catch (Exception ex)
            {
                _logger.LogError("66265-An error occurred while trying to lookup data for IP Address [{ip}]: {ex}", ip_list_data.ip_address, ex);

                return new Models.ApiRes_IPAddressLookupData
                {
                    ip_address = ip_list_data.ip_address,
                    country_iso3_code = string.Empty,
                    country_name = string.Empty,
                    timezone_id = string.Empty,
                    lat = 0,
                    lon = 0,
                    ip_id = ip_list_data.exec_order,
                    executed_at = DateTimeOffset.UtcNow,
                    last_error = ex.Message,
                };
            }
        }
        // ---------------------------------------------------------------------------------------------------------------------

    }
    // #####################################################################################################################################
}
