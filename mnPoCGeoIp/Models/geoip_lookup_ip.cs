using System;
using System.Collections.Generic;

namespace mnPoCGeoIp.Models;

public partial class geoip_lookup_ip
{
    public long glb_id { get; set; }

    public short exec_order { get; set; }

    public string ip_address { get; set; } = null!;

    public virtual geoip_lookup_ips_result? geoip_lookup_ips_result { get; set; }

    public virtual geoip_lookup_batch glb { get; set; } = null!;
}
