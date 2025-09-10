using System;
using System.Collections.Generic;

namespace mnPoCGeoIp.Models;

public partial class geoip_lookup_batch
{
    public long id { get; set; }

    public DateTimeOffset created { get; set; }

    public DateTimeOffset? process_started { get; set; }

    public DateTimeOffset? process_ended { get; set; }

    public short batch_count { get; set; }

    public virtual ICollection<geoip_lookup_ip> geoip_lookup_ips { get; set; } = new List<geoip_lookup_ip>();
}
