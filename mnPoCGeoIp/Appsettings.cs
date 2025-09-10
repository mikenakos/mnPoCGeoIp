using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace mnPoCGeoIp
{
    public class Appsettings
    {
        public string provider_geoip_access_token { get; set; } = default!;
        public required string provider_geoip_lookup_url { get; set; }
        public required string provider_geoip_auth_keyname { get; set; }

        // ---------------------------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public static Appsettings Current { get; private set; } = default!;
        // ---------------------------------------------------------------------------------------------------------------------
        public static void LoadSettings(WebApplicationBuilder builder)
        {
            // Avoid reload the settings
            if (Current != null)
                return;

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

            var tmpSettings = config.GetSection("mnPocSettings").Get<Appsettings>();
            if (tmpSettings == null)
                throw new NullReferenceException("67196-Settings seem null. Aborting!");

            Current = tmpSettings;

            // Check if there is an access token provided from an environment variable. It has higher priority than from the settings on the appsettings.json file
            var tmpAccessToken = builder.Configuration["PROVIDER_GEOIP_ACCESS_TOKEN"] ??
                Current.provider_geoip_access_token;

            if(string.IsNullOrEmpty(tmpAccessToken))
                throw new Exceptions.AppSettingNowFoundException("Access token for the GeoIP provider is not set. Please set it in the appsettings.json file or via an environment variable");

            Current.provider_geoip_access_token = tmpAccessToken;

        }
        // ---------------------------------------------------------------------------------------------------------------------
    }
}
