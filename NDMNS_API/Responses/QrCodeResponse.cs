namespace NDMNS_API.Responses
{
    public class QrCodeResponse
    {
        public string QrCode { get; set; } = string.Empty;
        public bool IsReady { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
