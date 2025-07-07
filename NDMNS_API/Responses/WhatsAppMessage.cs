namespace NDMNS_API.Responses
{
    public class WhatsAppMessage
    {
        public string Id { get; set; }

        public string Body { get; set; }

        public string From { get; set; }

        public string FromName { get; set; }

        public string To { get; set; }

        public bool IsGroup { get; set; }

        public string GroupName { get; set; }

        public DateTime Timestamp { get; set; }

        public bool HasMedia { get; set; }

        public bool IsReply { get; set; }

        public string RepliedToMessageId { get; set; }
    }
}
