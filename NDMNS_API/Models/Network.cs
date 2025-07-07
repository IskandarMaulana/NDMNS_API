namespace NDMNS_API.Models;

public partial class Network
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Ip { get; set; } = null!;

    public decimal Latency { get; set; }

    /// <summary>
    /// Status 1 = Down, 2 = Up, 3 = Intermittent
    /// </summary>
    public int Status { get; set; }

    public DateTime LastUpdate { get; set; }

    public string SiteId { get; set; } = null!;

    public string IspId { get; set; } = null!;

    public string Cid { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}

public class NetworkViewModel
{
    public string Id { get; set; } = null!;

    public string SiteId { get; set; } = null!;

    public string IspId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Ip { get; set; } = null!;

    public string Cid { get; set; } = null!;

    public decimal Latency { get; set; }

    public int Status { get; set; }

    public DateTime LastUpdate { get; set; }

    public string SiteName { get; set; } = null!;

    public string SiteLocation { get; set; } = null!;

    public string IspName { get; set; } = null!;
}
