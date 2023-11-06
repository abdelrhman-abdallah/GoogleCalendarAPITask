namespace CalenderAPITask.DTO
{
    public class ListQueryParamsDTO
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set;}
        public string? Query { get; set; }
        public int MaxResultSize { get; set; } = 10;
        public string? NextPageToken { get; set; }
    }
}
