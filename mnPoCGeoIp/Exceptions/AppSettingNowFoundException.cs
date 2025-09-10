namespace mnPoCGeoIp.Exceptions
{
    /// <summary>
    /// Exception thrown when an expected application setting is not found
    /// </summary>
    public class AppSettingNowFoundException : Exception
    {
        public AppSettingNowFoundException()
        {
        }
        public AppSettingNowFoundException(string message)
            : base(message)
        {
        }
        public AppSettingNowFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
