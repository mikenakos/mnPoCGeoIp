namespace mnPoCGeoIp.Services
{
    public static class ApiWebResponse
    {
        // ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// In case we need to return a successful response with data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Models.WebResponseInfo<T> Succedded<T>(T? data)
        {
            return new Models.WebResponseInfo<T>
            {
                success = true,
                data = data,
                error = null
            };
        }
        // ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// In case we need to return a failed response with an error message
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static Models.WebResponseInfo<object> Failed(string errorMessage)
        {
            return new Models.WebResponseInfo<object>
            {
                success = false,
                data = null,
                error = errorMessage
            };
        }
        // ---------------------------------------------------------------------------------------------------------------------
    }
}
