using mnPoCGeoIp.Infrastructure;
using mnPoCGeoIp.Services;

namespace mnPoCGeoIp.Extensions
{
    public static class PoCDiServicesExtensions
    {
        // ---------------------------------------------------------------------------------------------------------------------
        public static IServiceCollection AddLookupServices(this IServiceCollection services)
        {
            services.AddScoped<IWebClientService, WebClientService>();
            services.AddSingleton<IGeoIpLupJobQueue, GeoIpLupJobQueue>();
            services.AddScoped<IIpLookupService, IpLookupService>();
            services.AddScoped<IIpLookupServiceBatch, IpLookupService>();

            return services;
        }
        // ---------------------------------------------------------------------------------------------------------------------
    }
}
