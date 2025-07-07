namespace NDMNS_API.Responses
{
    public class SendRequest
    {
        public string NetworkId { get; set; }

        public string SiteId { get; set; }

        public string IspId { get; set; }

        public string? TicketNumber { get; set; }

        public int Status { get; set; }

        public string Image { get; set; }
    }
}
