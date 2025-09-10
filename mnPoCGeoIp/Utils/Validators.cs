namespace mnPoCGeoIp.Utils
{
    public class Validators
    {
        // ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Validates the IP Address format (IPv4 or IPv6).
        /// </summary>
        /// <param name="ipAddress">The IP Address string value</param>
        /// <returns>True when IP Address is OK</returns>
        public static bool IsValidIPAddress(string ipAddress)
        {
            return System.Net.IPAddress.TryParse(ipAddress, out _);
        }
        // ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Validates a list of IP Addresses. Returns an empty list if any invalid address found.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public static IEnumerable<string> ValidatedAddressesFactory(IEnumerable<string> addresses)
        {
            // Resolve non empty addresses
            var finalAddresses = addresses
                .Where(w => !string.IsNullOrEmpty(w.Trim()))
                .Distinct();

            var invalidAddresses = finalAddresses
                .Where(w => !IsValidIPAddress(w))
                .ToList();

            if (invalidAddresses.Count > 0)
                return [];

            // If no invalid address found. finalAddresses could also be empty
            return finalAddresses;
        }
        // ---------------------------------------------------------------------------------------------------------------------
    }
}
