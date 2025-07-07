namespace NDMNS_API.Models;

public partial class Email
{
    public string Id { get; set; } = null!;

    public string DowntimeId { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string Image { get; set; } = null!;

    public DateTime Date { get; set; }

    /// <summary>
    /// Type 1 = Downtime, 2 = Uptime, 3 = Intermittent
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Type 1 = Sent, 2 = Failed
    /// </summary>
    public int Status { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}

public partial class DetailEmailPic
{
    public string Id { get; set; } = null!;

    public string EmailId { get; set; } = null!;

    public string PicId { get; set; } = null!;

    /// <summary>
    /// Type of Email Recepient 1 = To, 2 = CC
    /// </summary>
    public int Type { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}

public partial class DetailEmailHelpdesk
{
    public string Id { get; set; } = null!;

    public string EmailId { get; set; } = null!;

    public string HelpdeskId { get; set; } = null!;

    /// <summary>
    /// Type of Email Recepient 1 = To, 2 = CC
    /// </summary>
    public int Type { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}

public class EmailViewModel
{
    public string Id { get; set; } = null!;

    public string DowntimeId { get; set; } = null!;

    /// <summary>
    /// Type 1 = Downtime, 2 = Uptime, 3 Intermittent
    /// </summary>
    public int Type { get; set; }

    public int Status { get; set; }

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string Image { get; set; } = null!;

    public DateTime Date { get; set; }

    public string DowntimeDescription { get; set; } = null!;

    public string DowntimeTicketNumber { get; set; } = null!;

    public List<DetailEmailPicViewModel> DetailEmailPics { get; set; } = [];

    public List<DetailEmailHelpdeskViewModel> DetailEmailHelpdesks { get; set; } = [];
}

public class DetailEmailPicViewModel
{
    public string Id { get; set; } = null!;
    public string EmailId { get; set; } = null!;
    public string PicId { get; set; } = null!;
    public int Type { get; set; }
    public string EmailAddress { get; set; } = null!;
    public string PicName { get; set; } = null!;
}

public class DetailEmailHelpdeskViewModel
{
    public string Id { get; set; } = null!;
    public string EmailId { get; set; } = null!;
    public string HelpdeskId { get; set; } = null!;
    public int Type { get; set; }
    public string EmailAddress { get; set; } = null!;
    public string HelpdeskName { get; set; } = null!;
}
