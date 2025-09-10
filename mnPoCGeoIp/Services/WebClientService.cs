using System.Text.Json;
using System.Text.Json.Nodes;

namespace mnPoCGeoIp.Services
{
    // #####################################################################################################################################
    public interface IWebClientService
    {
        /// <summary>
        /// Generic method to get JSON data from a URL with authorization header
        /// </summary>
        /// <param name="URL">The URL of the remote end-point</param>
        /// <param name="authData">The key/value object with the authorization key/token</param>
        /// <returns>The deserialized object to return</returns>
        /// <exception cref="NullReferenceException"></exception>
        Task<JsonNode> GetJsonDataAsync(string URL, KeyValuePair<string, string> authData);
    }
    // #####################################################################################################################################
    /// <summary>
    /// The interface geo ip implementation class
    /// </summary>
    public class WebClientService : IWebClientService
    {
        // ---------------------------------------------------------------------------------------------------------------------
#if BYPASS_REMOTE_SERVICE_CALL
        public Task<JsonNode> GetJsonDataAsync(string URL, KeyValuePair<string, string> authData)
        {
            http_error = null;
            // Just for the dev environment, bypass the remote call and return a static JSON payload
            string jsonString = "{\"data\":{\"ip\":\"1.1.1.1\",\"hostname\":null,\"type\":\"v4\",\"range_type\":{\"type\":\"PUBLIC\",\"description\":\"Public address\"},\"connection\":{\"asn\":13335,\"organization\":\"Cloudflare, Inc.\",\"isp\":\"Cloudflare, Inc\",\"range\":\"1.1.1.0\\/24\"},\"location\":{\"geonames_id\":101934019,\"latitude\":-27.467540740966797,\"longitude\":153.02809143066406,\"zip\":\"4000\",\"continent\":{\"code\":\"OC\",\"name\":\"Oceania\",\"name_translated\":\"Oceania\",\"geonames_id\":6255151,\"wikidata_id\":\"Q538\"},\"country\":{\"alpha2\":\"AU\",\"alpha3\":\"AUS\",\"calling_codes\":[\"+61\"],\"currencies\":[{\"symbol\":\"AU$\",\"name\":\"Australian Dollar\",\"symbol_native\":\"$\",\"decimal_digits\":2,\"rounding\":0,\"code\":\"AUD\",\"name_plural\":\"Australian dollars\",\"type\":\"fiat\"}],\"emoji\":\"🇦🇺\",\"ioc\":\"AUS\",\"languages\":[{\"name\":\"English\",\"name_native\":\"English\"}],\"name\":\"Australia\",\"name_translated\":\"Australia\",\"timezones\":[\"Australia\\/Lord_Howe\",\"Antarctica\\/Macquarie\",\"Australia\\/Hobart\",\"Australia\\/Currie\",\"Australia\\/Melbourne\",\"Australia\\/Sydney\",\"Australia\\/Broken_Hill\",\"Australia\\/Brisbane\",\"Australia\\/Lindeman\",\"Australia\\/Adelaide\",\"Australia\\/Darwin\",\"Australia\\/Perth\",\"Australia\\/Eucla\"],\"is_in_european_union\":false,\"fips\":\"AS\",\"geonames_id\":85632793,\"hasc_id\":\"AU\",\"wikidata_id\":\"Q408\"},\"city\":{\"fips\":null,\"alpha2\":null,\"geonames_id\":101934019,\"hasc_id\":null,\"wikidata_id\":\"Q34932\",\"name\":\"Brisbane\",\"name_translated\":\"Brisbane\"},\"region\":{\"fips\":\"AS04\",\"alpha2\":\"AU-QLD\",\"geonames_id\":85681463,\"hasc_id\":\"AU.QL\",\"wikidata_id\":\"Q36074\",\"name\":\"Queensland\",\"name_translated\":\"Queensland\"}},\"tlds\":[\".au\"],\"timezone\":{\"id\":\"Australia\\/Brisbane\",\"current_time\":\"2025-09-08T05:45:56+10:00\",\"code\":\"AEST\",\"is_daylight_saving\":false,\"gmt_offset\":36000},\"security\":{\"is_anonymous\":null,\"is_datacenter\":null,\"is_vpn\":null,\"is_bot\":null,\"is_abuser\":null,\"is_known_attacker\":null,\"is_proxy\":null,\"is_spam\":null,\"is_tor\":null,\"proxy_type\":null,\"is_icloud_relay\":null,\"threat_score\":null},\"domains\":{\"count\":null,\"domains\":[]}}}";
#else
        public async Task<JsonNode> GetJsonDataAsync(string URL, KeyValuePair<string, string> authData)
        {
            using HttpClient client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Get, URL);
            req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            req.Headers.Add(authData.Key, authData.Value);

            using var httpRes = await client.SendAsync(req);
            httpRes.EnsureSuccessStatusCode();
            var jsonString = await httpRes.Content.ReadAsStringAsync();
#endif
            JsonNode? node = null;
            if (string.IsNullOrEmpty(jsonString) || (node = JsonNode.Parse(jsonString)) == null)
                throw new NullReferenceException("16574-Json string is empty or could not deserialize string payload");

#if BYPASS_REMOTE_SERVICE_CALL
            return Task.FromResult(node);
#else
            return node;
#endif
        }
        // ---------------------------------------------------------------------------------------------------------------------
    }
    // #####################################################################################################################################
}
