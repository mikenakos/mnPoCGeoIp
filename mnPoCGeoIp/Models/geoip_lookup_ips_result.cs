using System;
using System.Collections.Generic;

namespace mnPoCGeoIp.Models;

public partial class geoip_lookup_ips_result
{
    public long glb_id { get; set; }

    public short exec_order { get; set; }

    public DateTimeOffset updated { get; set; }

    public string? last_error { get; set; }

    public string? country_iso3_code { get; set; }

    public string? country_name { get; set; }

    public string timezone_id { get; set; } = null!;

    public decimal lat { get; set; }

    public decimal lon { get; set; }

    public virtual geoip_lookup_ip geoip_lookup_ip { get; set; } = null!;
}
