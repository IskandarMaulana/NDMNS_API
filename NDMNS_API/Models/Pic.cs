namespace NDMNS_API.Models;

public partial class Pic
{
    public string Id { get; set; }

    public string SiteId { get; set; } = null!;

    public string Nrp { get; set; } = null!;

    public string Name { get; set; } = null!;

    /// <summary>
    /// Role 1 = PIC Site, 2 = PIC Head Office,
    /// </summary>
    public int Role { get; set; }

    public string WhatsappNumber { get; set; } = null!;

    public string? EmailAddress { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}

public class PicViewModel
{
    public string Id { get; set; } = null!;

    public string SiteId { get; set; } = null!;

    public string Nrp { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Role { get; set; }

    public string WhatsappNumber { get; set; } = null!;

    public string? EmailAddress { get; set; } = null!;

    public string SiteName { get; set; } = null!;
}
