namespace mnPoCGeoIp.Models
{
    // #####################################################################################################################################
    /// <summary>
    /// Class to map the special results from the database to return the batch status info
    /// </summary>
    public class BatchStatusDbData
    {
        public long batch_id { get; set; }
        public short batch_count { get; set; }
        public short completed_count { get; set; }
        public DateTimeOffset? eta { get; set; }
        public DateTimeOffset? batch_start_time { get; set; }
        public DateTimeOffset? batch_end_time { get; set; }
        
    }
    // #####################################################################################################################################
}
