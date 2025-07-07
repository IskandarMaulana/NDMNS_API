namespace NDMNS_API.Models;

public partial class Session
{
    public string Id { get; set; }

    public string UserId { get; set; }

    public string Device { get; set; }

    public string Ip { get; set; }

    public DateTime LoginTime { get; set; }

    public DateTime LastAccess { get; set; }
}
