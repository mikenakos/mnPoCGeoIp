
namespace mnPoCGeoIp.Services
{

    public class BackgroundProcessingService : BackgroundService
    {
        private readonly ILogger<BackgroundProcessingService> _logger;
        private readonly Infrastructure.IGeoIpLupJobQueue _geoQueue;
        //private readonly IIpLookupServiceBatch _iplookupSvc;
        private readonly IServiceProvider _services;

        // ---------------------------------------------------------------------------------------------------------------------
        public BackgroundProcessingService(
            ILogger<BackgroundProcessingService> logger,
            //IIpLookupServiceBatch iplookupSvc,
            Infrastructure.IGeoIpLupJobQueue geoQueue,
            IServiceProvider services)
        {
            _logger = logger;
            //_iplookupSvc = iplookupSvc;
            _geoQueue = geoQueue;
            _services = services;
        }
        // ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// The overriden method that does the background processing, listening to the queue for new jobs
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Processing Service is starting");

            // Stick to this loop until a stop is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                Models.GeoIpBatchLookupJob job;

                try
                {
                    // This dequeus a job or waits until a job is available
                    // Note: Job processing is done sequentially
                    job = await _geoQueue.DequeueAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Background Processing Service is stopping due to cancellation request");
                    break;
                }

                try
                {
                    // Manually create a scope to resolve the scoped services, because this class is a singleton
                    // and cannot directly consume scoped services
                    using var scope = _services.CreateScope();
                    var iplookupSvc = scope.ServiceProvider.GetRequiredService<IIpLookupServiceBatch>();

                    // Process each of the IP address and store the results to the database
                    await iplookupSvc.LookupAndStoreResultsAsync(job.batch_id, job.ip_addresses);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "96687-Error occurred while executing lookup for job {jobId}", job.batch_id);
                }

            }

            _logger.LogInformation("Background Processing Service is stopped");
        }
        // ---------------------------------------------------------------------------------------------------------------------
    }
}
