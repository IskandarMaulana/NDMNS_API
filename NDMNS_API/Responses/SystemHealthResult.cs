namespace NDMNS_API.Responses
{
    public class SystemHealthResult
    {
        public DateTime CheckTime { get; set; }
        public HealthCheckResult Database { get; set; }
        public HealthCheckResult Internet { get; set; }
        public HealthCheckResult NodeJsService { get; set; }
        public HealthCheckResult WhatsAppService { get; set; }
    }
}
