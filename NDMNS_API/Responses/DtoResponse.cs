namespace NDMNS_API.Responses
{
    public class DtoResponse<T>
    {
        public int status { get; set; }
        public String? message { get; set; }
        public T? data { get; set; }
    }
}
