using Microsoft.EntityFrameworkCore;
using mnPoCGeoIp.Models;

namespace mnPoCGeoIp.DataBase
{
    // #####################################################################################################################################
    public interface IIpAddressRepository
    {
        /// <summary>
        /// Store the batch (header) and the associated IP addresses to be looked up
        /// </summary>
        /// <param name="ip_addresses">The IP address list</param>
        /// <returns></returns>
        Task<(long batch_id, List<geoip_lookup_ip> ip_addresses)> StoreIpAddressesAsync(IEnumerable<string> ip_addresses);

        /// <summary>
        /// Store the fetched data, containing the geo-ip details, for a specific IP address on a specific batch
        /// </summary>
        /// <param name="batch_id"></param>
        /// <param name="ip_data"></param>
        /// <returns></returns>
        Task SaveFetchedDataAsync(long batch_id, ApiRes_IPAddressLookupData ip_data);

        /// <summary>
        /// Updates the batch start or end time, depending on the update type parameter
        /// </summary>
        /// <param name="batch_id">The batch_id</param>
        /// <param name="update_type"><see cref="BATCH_UPDATE_TYPES"/></param>
        /// <returns></returns>
        Task UpdateBatchStartOrEndAsync(long batch_id, BATCH_UPDATE_TYPES update_type);

        /// <summary>
        /// Resolves the batch status, including the ETA and the completion percentage
        /// </summary>
        /// <param name="batch_id"></param>
        /// <returns></returns>
        Task<BatchStatusDbData> ResolveBatchStatusAsync(long batch_id);
    }
    // #####################################################################################################################################
    public class IpAddressRepository : IIpAddressRepository
    {
        private readonly AppDbContext _db;
        // ---------------------------------------------------------------------------------------------------------------------
        public IpAddressRepository(AppDbContext db)
        {
            _db = db;
        }
        // ---------------------------------------------------------------------------------------------------------------------
        public Task<(long batch_id, List<geoip_lookup_ip> ip_addresses)> StoreIpAddressesAsync(IEnumerable<string> ip_addresses)
        {
            // Resolve the batch count
            short batchCount = (short)ip_addresses.Count();

            var newBatch = new geoip_lookup_batch()
            {
                batch_count = batchCount,
                created = DateTimeOffset.UtcNow,
            };

            // Each IP address on the batch has a distinct execution order
            int execOrderCounter = 0;
            foreach(string ip in ip_addresses)
            {
                execOrderCounter++;
                newBatch.geoip_lookup_ips.Add(new geoip_lookup_ip {
                    ip_address = ip,
                    exec_order = (short)execOrderCounter,
                });
            }

            _db.geoip_lookup_batches.Add(newBatch);

            // Finally store changes. No need to take extra care about the transaction/rollback, as EF will handle that in case of error
            _db.SaveChanges();

            return Task.FromResult((newBatch.id, newBatch.geoip_lookup_ips.ToList()));
        }
        // ---------------------------------------------------------------------------------------------------------------------
        public Task SaveFetchedDataAsync(long batch_id, ApiRes_IPAddressLookupData ip_data)
        {
            _db.geoip_lookup_ips_results.Add(new geoip_lookup_ips_result {
                glb_id = batch_id,
                exec_order = ip_data.ip_id,
                updated = ip_data.executed_at,
                country_iso3_code = ip_data.country_iso3_code,
                country_name = ip_data.country_name,
                timezone_id = ip_data.timezone_id,
                lat = ip_data.lat,
                lon = ip_data.lon,
            });

            _db.SaveChanges();

            return Task.CompletedTask;
        }
        // ---------------------------------------------------------------------------------------------------------------------
        public Task UpdateBatchStartOrEndAsync(long batch_id, BATCH_UPDATE_TYPES update_type)
        {
            _db.geoip_lookup_batches
                .Where(w => w.id == batch_id)
                .ExecuteUpdate(
                    u => u.SetProperty(
                        p => (update_type == BATCH_UPDATE_TYPES.STARTED) ? p.process_started : p.process_ended, _ => DateTimeOffset.UtcNow));


            return Task.CompletedTask;
        }
        // ---------------------------------------------------------------------------------------------------------------------
        public Task<BatchStatusDbData> ResolveBatchStatusAsync(long batch_id)
        {
            var nowUtc = DateTimeOffset.UtcNow;
            // Fetch the batch data along with the completed count from the DB
            var lookupRes = _db.geoip_lookup_batches
                .AsNoTracking()
                .Where(w => w.id == batch_id)
                .Select(s => new BatchStatusDbData
                {
                    batch_id = s.id,
                    batch_count = s.batch_count,
                    completed_count = (short)_db.geoip_lookup_ips_results.Count(rw => rw.glb_id == batch_id),
                    //elapsed_seconds =  EF.Functions.DateDiffSecond(s.process_started!.Value, nowUtc),
                    batch_end_time = s.process_ended,
                    batch_start_time = s.process_started,
                })
                .SingleOrDefault();

            if(lookupRes == null)
                throw new Exceptions.DbRowNotFoundException($"Batch with id [{batch_id}] was not found");

            else if (lookupRes.batch_end_time != null)
            {
                // Batch finished
                lookupRes.eta = lookupRes.batch_end_time!.Value;
            }
            else if (lookupRes.completed_count > 0)
            {
                // Batch is incomplete
                var elapsed = DateTimeOffset.UtcNow - lookupRes.batch_start_time!.Value;
                var avgPerIp = elapsed.TotalSeconds / lookupRes.completed_count;
                var remainingIpCount = lookupRes.batch_count - lookupRes.completed_count;
                lookupRes.eta = nowUtc.AddSeconds(avgPerIp * remainingIpCount);
            }
            else
            {
                // Processing is not yet started or the first ip lookup is not yet complete
                // Give an arbitary ETA of 2 seconds for each ip on the batch + a grace period
                // We could have also give a historical average
                lookupRes.eta = nowUtc.AddSeconds((2 * lookupRes.batch_count) + 5);
            }

            return Task.FromResult(lookupRes);
        }
        // ---------------------------------------------------------------------------------------------------------------------
    }
    // #####################################################################################################################################
}
