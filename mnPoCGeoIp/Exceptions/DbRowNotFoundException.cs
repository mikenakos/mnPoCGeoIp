namespace mnPoCGeoIp.Exceptions
{
    /// <summary>
    /// Exception thrown when a database row is not found
    /// </summary>
    public class DbRowNotFoundException : Exception
    {
        public DbRowNotFoundException()
        {
        }
        public DbRowNotFoundException(string message)
            : base(message)
        {
        }
        public DbRowNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
