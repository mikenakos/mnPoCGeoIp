using mnPoCGeoIp.Models;
using System.Threading.Channels;

namespace mnPoCGeoIp.Infrastructure
{
    // #####################################################################################################################################
    public interface IGeoIpLupJobQueue
    {
        /// <summary>
        /// Enqueue/push the message to the channel/queue
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c_token"></param>
        /// <returns></returns>
        Task EnqueueAsync(GeoIpBatchLookupJob job, CancellationToken c_token = default);

        /// <summary>
        /// Dequeue/pop the message from the channel/queue - One message at a time
        /// </summary>
        /// <param name="c_token"></param>
        /// <returns></returns>
        Task<GeoIpBatchLookupJob> DequeueAsync(CancellationToken c_token);
    }
    // #####################################################################################################################################
    /// <summary>
    /// A thread-safe queue for managing GeoIP batch lookup jobs using channels
    /// </summary>
    public class GeoIpLupJobQueue : IGeoIpLupJobQueue
    {
        private readonly Channel<GeoIpBatchLookupJob> _channel;
        // ---------------------------------------------------------------------------------------------------------------------
        public GeoIpLupJobQueue(int capacity = 50)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false,
            };

            _channel = Channel.CreateBounded<GeoIpBatchLookupJob>(options);
        }
        // ---------------------------------------------------------------------------------------------------------------------
        public Task<GeoIpBatchLookupJob> DequeueAsync(CancellationToken c_token)
            => _channel.Reader.ReadAsync(c_token).AsTask();
        // ---------------------------------------------------------------------------------------------------------------------
        public Task EnqueueAsync(GeoIpBatchLookupJob job, CancellationToken c_token = default)
            => _channel.Writer.WriteAsync(job, c_token).AsTask();
        // ---------------------------------------------------------------------------------------------------------------------
    }
    // #####################################################################################################################################
}
