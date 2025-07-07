namespace NDMNS_API.Responses
{
    public class PingResult
    {
        public string Hostname { get; set; } = string.Empty;
        public int Status { get; set; } // 1 = Down, 2 = Up, 3 = Intermittent
        public decimal AverageLatency { get; set; }
        public double SuccessPercentage { get; set; }
        public string Response { get; set; } = string.Empty;
    }
}
