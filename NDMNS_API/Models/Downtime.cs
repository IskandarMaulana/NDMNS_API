namespace NDMNS_API.Models;

public partial class Downtime
{
    public string Id { get; set; } = null!;

    public string NetworkId { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string TicketNumber { get; set; } = null!;

    public DateTime Date { get; set; }

    public DateTime Start { get; set; }

    public DateTime? End { get; set; }

    /// <summary>
    /// Category 1 = INTERNAL, 2 = ISP
    /// </summary>
    public int Category { get; set; }

    /// <summary>
    /// Sub Category 1 = Cabling; 2 = Electricity; 3 = Full Traffic; 4 = Genset / Power Backup; 5 = Link Disable;
    /// 6 = Maintenance Internal; 7 = No Data Package; 8 = Radio Issue; 9 = Tower Issue; 10 = FO Cut; 11 = Signal / Radio Degraded;
    /// 12 = BTS Issue; 13 = PLN Issue; 14 = Maintenance Perangkat ISP; 15 = Router / Modem Issue; 16 = CP Issue
    /// </summary>
    public int Subcategory { get; set; }

    public string? Action { get; set; }

    /// <summary>
    /// Status 1 = Ongoing, 2 = Resolved
    /// </summary>
    public int Status { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}

public partial class DowntimeViewModel
{
    public string Id { get; set; } = null!;

    public string NetworkId { get; set; } = null!;

    public string NetworkName { get; set; } = null!;

    public string SiteId { get; set; } = null!;

    public string SiteName { get; set; } = null!;

    public string IspId { get; set; } = null!;

    public string IspName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string TicketNumber { get; set; } = null!;

    public DateTime Date { get; set; }

    public DateTime Start { get; set; }

    public DateTime? End { get; set; }

    public int Category { get; set; }

    public int Subcategory { get; set; }

    public string? Action { get; set; }

    public int Status { get; set; }
}
