namespace NDMNS_API.Models;

public partial class Isp
{
    public string Id { get; set; }

    public string Name { get; set; } = null!;

    public string WhatsappGroup { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}

public class IspViewModel
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string WhatsappGroup { get; set; } = null!;

    public string WhatsappGroupName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;
}
