namespace NDMNS_API.Models;

public partial class Helpdesk
{
    public string Id { get; set; }

    public string IspId { get; set; } = null!;

    public string Name { get; set; } = null!;

    /// <summary>
    /// Role 1 = Helpdesk, 2 = Helpdesk Lead, etc.
    /// </summary>
    public int Role { get; set; }

    public string WhatsappNumber { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}

public class HelpdeskViewModel
{
    public string Id { get; set; } = null!;

    public string IspId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Role { get; set; }

    public string WhatsappNumber { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string IspName { get; set; } = null!;
}
