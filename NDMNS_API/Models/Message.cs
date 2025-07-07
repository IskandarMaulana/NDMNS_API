namespace NDMNS_API.Models;

public partial class Message
{
    public string Id { get; set; }

    public string DowntimeId { get; set; }

    public DateTime Date { get; set; }

    public string Recipient { get; set; }

    /// <summary>
    /// RecipientType 1 = Site, 2 = ISP
    /// </summary>
    public int RecipientType { get; set; }

    public string MessageId { get; set; }

    public string Text { get; set; }

    public string Image { get; set; }

    /// <summary>
    /// Type 1 = Downtime, 2 = Uptime, 3 Intermittent
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Category 1 = Alert, 2 = Response, 3 = Update
    /// </summary>
    public int Category { get; set; }

    /// <summary>
    /// Level is how many attempt to sent alert for a downtime
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Status 1 = Sent, 2 = Responded, 3 = Failed Sent
    /// </summary>
    public int Status { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}

public class MessageViewModel
{
    public string Id { get; set; } = null!;

    public string DowntimeId { get; set; } = null!;

    public DateTime Date { get; set; }

    public string Recipient { get; set; } = null!;

    public int RecipientType { get; set; }

    public string MessageId { get; set; } = null!;

    public string Text { get; set; } = null!;

    public string Image { get; set; }

    public int Type { get; set; }

    public int Category { get; set; }

    public int Level { get; set; }

    public int Status { get; set; }

    public string DowntimeDescription { get; set; } = null!;

    public string DowntimeTicketNumber { get; set; } = null!;

    public string RecipientName { get; set; } = null!;
}
