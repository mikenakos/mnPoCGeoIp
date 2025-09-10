using System.Text.Json.Serialization;

namespace mnPoCGeoIp.Models
{
    // #####################################################################################################################################
    public class ApiRes_IPAddressLookupData
    {
        public required string ip_address { get; set; }
        public required string country_iso3_code { get; set; }
        public required string country_name { get; set; }
        public required string timezone_id { get; set; }
        public decimal lat { get; set; }
        public decimal lon { get; set; }

        private string? _last_error = null;
        public string? last_error
        {
            get => _last_error;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    _last_error = null;
                else
                    _last_error = value.Substring(0, 128);
            }
        }

        [JsonIgnore]
        public short ip_id { get; set; }

        [JsonIgnore]
        public DateTimeOffset executed_at { get; set; }
    }
    // #####################################################################################################################################
    public class ApiRes_BatchLookupResult
    {
        public required string batch_status_url { get; set; }
        public long batch_id { get; set; }
    }
    // #####################################################################################################################################
    public class ApiRes_BatchStatus
    {
        public long batch_id { get; set; }
        public double percentage_completed { get; set; } = 0;
        public int ip_addresses_count { get; set; }
        public DateTimeOffset? eta { get; set; }
    }
    // #####################################################################################################################################
    public class WebResponseInfo<T>
    {
        public bool success { get; set; } = false;
        public T? data { get; set; }
        public string? error { get; set; }
    }
    // #####################################################################################################################################
}
