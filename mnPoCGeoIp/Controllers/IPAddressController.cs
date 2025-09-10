using Microsoft.AspNetCore.Mvc;
using mnPoCGeoIp.DataBase;
using mnPoCGeoIp.Services;

namespace mnPoCGeoIp.Controllers
{
    /// <summary>
    /// API Controller to handle IP Address related operations
    /// Intentionally left without authorization, since the requirements do not specify any auth mechanism
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class IPAddressController : ControllerBase
    {
        // ---------------------------------------------------------------------------------------------------------------------
        private readonly ILogger<IPAddressController> _logger;
        private readonly IIpLookupService ipLookupService;
        private readonly IIpAddressRepository _dbIpRepo;
        private readonly Infrastructure.IGeoIpLupJobQueue _batchQueue;
        private readonly LinkGenerator _links;

        public IPAddressController(
            ILogger<IPAddressController> logger,
            IIpLookupService lookupService,
            IIpAddressRepository dbIpAddrRepo,
            Infrastructure.IGeoIpLupJobQueue batchQueue,
            LinkGenerator links)
        {
            _logger = logger;
            ipLookupService = lookupService;
            _dbIpRepo = dbIpAddrRepo;
            _batchQueue = batchQueue;
            _links = links;
        }
        // ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 1. Gets the geolocation information for a single IP address.
        /// </summary>
        /// <param name="ip_address">The IP Address to look-up</param>
        /// <returns></returns>
        [ActionName("Lookup")]
        [HttpGet("{ip_address}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.WebResponseInfo<Models.ApiRes_IPAddressLookupData>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Models.WebResponseInfo<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Models.ApiRes_IPAddressLookupData>> GetSingleIPAddressInfo(string ip_address)
        {
            _logger.LogInformation("GET-SINGLE-IP|CLI-IP:{cli_ip}|IP:{ip}", HttpContext.Connection.RemoteIpAddress, ip_address);

            // Make some validations
            if (!Utils.Validators.IsValidIPAddress(ip_address))
            {
                return BadRequest(ApiWebResponse.Failed("Invalid IP Address format"));
            }

            try
            {
                // Try to fetch the lookup result from the remote service
                var ip_data = await ipLookupService.StoreIpAddrAndWriteResultsAsync(new List<string>() { ip_address });

                // All done. Return the result
                return Ok(ApiWebResponse.Succedded(ip_data.First()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "23984-Error occurred while trying to make a single lookup");
                return StatusCode(500, "Internal Server Error");
            }
        }
        // ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 2. Stores the IP addresses to be looked-up in batch mode and enqueues the request for processing
        /// </summary>
        /// <param name="ip_addresses">The list of IP addresses</param>
        /// <returns></returns>
        [ActionName("SaveIpBatch")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.WebResponseInfo<Models.ApiRes_BatchLookupResult>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Models.WebResponseInfo<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Models.ApiRes_BatchLookupResult>> PostIpAddressBatch2Lookup(List<string> ip_addresses)
        {
            _logger.LogInformation("GET-BULK-IP|CLI-IP:{cli_ip}", HttpContext.Connection.RemoteIpAddress);
            
            // Make some validations
            var validatedAddresses = Utils.Validators.ValidatedAddressesFactory(ip_addresses);
            if (validatedAddresses.Count() == 0)
            {
                return BadRequest(ApiWebResponse.Failed("IP addresses validation error"));
            }

            try
            {
                // First of all, store the batch data to the database
                var storeRes = await _dbIpRepo.StoreIpAddressesAsync(validatedAddresses);

                // In a production system, normally this would be the point that we push the batch_id to a message broker like RabbitMQ or Kafka for 2 reasons
                // 1. Jobs like this, a background processing, is allways dispatched asynchronously to different processes/workers (same or different server)
                //    to avoid stressing the API with long running tasks
                // 2. By following this approach, we are on track with the microservices architecture principles, segregating the tasks and the responsibilities

                // Enqueue the batch for processing (in this PoC, we will process it directly, in-process)
                var job = new Models.GeoIpBatchLookupJob(storeRes.batch_id, storeRes.ip_addresses);

                var ct = HttpContext.RequestAborted;
                await _batchQueue.EnqueueAsync(job, ct);

                string? statusFullURL = _links.GetUriByAction(
                    HttpContext,
                    action: "BatchStatus",
                    controller: "IPAddress",
                    values: new { storeRes.batch_id });

                return Ok(ApiWebResponse.Succedded(new Models.ApiRes_BatchLookupResult() {
                    batch_id = storeRes.batch_id,
                    batch_status_url = statusFullURL!,
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "19904-Error occurred while trying to store bulk ip addresses for lookup");
                return StatusCode(500, "Internal Server Error");
            }
        }
        // ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 3. Gets the status of a batch processed/processing in the background
        /// </summary>
        /// <param name="batch_id">The batch id</param>
        /// <returns></returns>
        [ActionName("BatchStatus")]
        [HttpGet("{batch_id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Models.WebResponseInfo<Models.ApiRes_BatchStatus>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Models.WebResponseInfo<object>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Models.WebResponseInfo<Models.ApiRes_BatchStatus>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Models.ApiRes_BatchStatus>> GetBatchStatus(long batch_id)
        {
            _logger.LogInformation("GET-BLK-STATUS|CLI-IP:{cli_ip}|Batch:{batch_id}", HttpContext.Connection.RemoteIpAddress, batch_id);
            try
            {
                var dbRes = await _dbIpRepo.ResolveBatchStatusAsync(batch_id);

                var prc_completed = Math.Round((((double)dbRes.completed_count / (double)dbRes.batch_count) * 100), 2);

                return Ok(ApiWebResponse.Succedded(new Models.ApiRes_BatchStatus()
                {
                    batch_id = batch_id,
                    percentage_completed = prc_completed,
                    eta = dbRes.eta,
                    ip_addresses_count = dbRes.batch_count,
                }));
            }
            catch (Exceptions.DbRowNotFoundException)
            {
                return NotFound(ApiWebResponse.Failed($"batch_id [{batch_id}] not found in the database"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "59859-Error occurred while trying to get batch status with id: {batch_id}", batch_id);
                return StatusCode(500, "Internal Server Error");
            }
        }
        // ---------------------------------------------------------------------------------------------------------------------
    }
}
