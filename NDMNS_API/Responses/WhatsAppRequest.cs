using System.Text.Json.Serialization;

namespace NDMNS_API.Responses
{
    /// <summary>
    /// Request model untuk mengirim pesan WhatsApp melalui endpoint /api/whatsapp/send
    /// </summary>
    public class WhatsAppRequest
    {
        /// <summary>
        /// Nomor tujuan, bisa berupa nomor telepon atau ID grup
        /// </summary>
        [JsonPropertyName("to")]
        public string To { get; set; }

        /// <summary>
        /// Isi pesan atau caption untuk media
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Tipe pengiriman: "chat" untuk personal atau "group" untuk grup
        /// </summary>
        [JsonPropertyName("sendType")]
        public string SendType { get; set; } = "chat";

        /// <summary>
        /// Tipe pesan: "text", "media", "location", "poll", "buttons", "list", "contacts"
        /// </summary>
        [JsonPropertyName("messageType")]
        public string MessageType { get; set; } = "text";

        /// <summary>
        /// Konten tambahan untuk pesan (berbeda untuk setiap tipe pesan)
        /// </summary>
        [JsonPropertyName("contents")]
        public object Contents { get; set; }

        /// <summary>
        /// Opsi tambahan untuk pesan
        /// </summary>
        [JsonPropertyName("options")]
        public object Options { get; set; }
    }

    /// <summary>
    /// Opsi untuk berbagai jenis pesan WhatsApp
    /// </summary>
    public class MessageOption
    {
        [JsonPropertyName("mentions")]
        public string[] Mentions { get; set; }

        [JsonPropertyName("quotedMessageId")]
        public string QuotedMessageId { get; set; }
    }

    /// <summary>
    /// Opsi untuk berbagai jenis pesan WhatsApp
    /// </summary>
    public class MessageContent
    {
        // Media options
        [JsonPropertyName("messageMedia")]
        public MessageMedia MessageMedia { get; set; } = new MessageMedia();

        // Location options
        [JsonPropertyName("location")]
        public LocationInfo Location { get; set; } = new LocationInfo();

        // Poll options
        [JsonPropertyName("poll")]
        public PollInfo Poll { get; set; } = new PollInfo();

        // Buttons options
        [JsonPropertyName("buttons")]
        public List<ButtonInfo> Buttons { get; set; } = new List<ButtonInfo>();

        [JsonPropertyName("footer")]
        public string Footer { get; set; } = "";

        // List options
        [JsonPropertyName("list")]
        public ListInfo List { get; set; } = new ListInfo();

        // Contacts options
        [JsonPropertyName("contacts")]
        public List<ContactInfo> Contacts { get; set; } = new List<ContactInfo>();
    }

    /// <summary>
    /// Informasi lokasi untuk pesan tipe location
    /// </summary>
    public class MessageMedia
    {
        [JsonPropertyName("isBase64")]
        public bool? IsBase64 { get; set; } = null;

        [JsonPropertyName("media")]
        public string Media { get; set; } = null;

        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = null;

        [JsonPropertyName("filename")]
        public string Filename { get; set; } = null;
    }

    /// <summary>
    /// Informasi lokasi untuk pesan tipe location
    /// </summary>
    public class LocationInfo
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// Informasi polling untuk pesan tipe poll
    /// </summary>
    public class PollInfo
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("options")]
        public List<string> Options { get; set; } = new List<string>();

        [JsonPropertyName("allowMultipleAnswers")]
        public bool AllowMultipleAnswers { get; set; } = false;
    }

    /// <summary>
    /// Informasi tombol untuk pesan tipe buttons
    /// </summary>
    public class ButtonInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("body")]
        public string Body { get; set; } = "";
    }

    /// <summary>
    /// Informasi list untuk pesan tipe list
    /// </summary>
    public class ListInfo
    {
        [JsonPropertyName("body")]
        public string Body { get; set; } = "";

        [JsonPropertyName("buttonText")]
        public string ButtonText { get; set; } = "";

        [JsonPropertyName("sections")]
        public List<ListSection> Sections { get; set; } = new List<ListSection>();

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("footer")]
        public string Footer { get; set; } = "";
    }

    /// <summary>
    /// Informasi section untuk list
    /// </summary>
    public class ListSection
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("rows")]
        public List<ListRow> Rows { get; set; } = new List<ListRow>();
    }

    /// <summary>
    /// Informasi row untuk section dalam list
    /// </summary>
    public class ListRow
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// Informasi kontak untuk pesan tipe contacts
    /// </summary>
    public class ContactInfo
    {
        [JsonPropertyName("name")]
        public ContactName Name { get; set; } = new ContactName();

        [JsonPropertyName("phones")]
        public List<ContactPhone> Phones { get; set; } = new List<ContactPhone>();

        [JsonPropertyName("emails")]
        public List<ContactEmail> Emails { get; set; } = new List<ContactEmail>();
    }

    /// <summary>
    /// Informasi nama kontak
    /// </summary>
    public class ContactName
    {
        [JsonPropertyName("formatted")]
        public string Formatted { get; set; } = "";

        [JsonPropertyName("first")]
        public string First { get; set; } = "";

        [JsonPropertyName("last")]
        public string Last { get; set; } = "";
    }

    /// <summary>
    /// Informasi nomor telepon kontak
    /// </summary>
    public class ContactPhone
    {
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "mobile"; // mobile, home, work, etc.
    }

    /// <summary>
    /// Informasi email kontak
    /// </summary>
    public class ContactEmail
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "home"; // home, work, etc.
    }
}
