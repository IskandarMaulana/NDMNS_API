namespace NDMNS_API.Models;

public partial class User
{
    public string Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Nrp { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? Email { get; set; }

    public string? WhatsApp { get; set; }

    /// <summary>
    /// WhatsApp Client Status 1 = Disconnected, 2 Connected
    /// </summary>
    public int WhatsAppClient { get; set; }

    /// <summary>
    /// User Status 1 = Not Active, 2 Active
    /// </summary>
    public int Status { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}

public class UserViewModel
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Nrp { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string WhatsApp { get; set; } = null!;

    public int WhatsAppClient { get; set; }

    public int Status { get; set; }
}

public class LoginRequest
{
    public string Nrp { get; set; } = null!;

    public string Password { get; set; } = null!;
}
