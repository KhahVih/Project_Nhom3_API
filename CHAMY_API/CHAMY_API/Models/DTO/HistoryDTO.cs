namespace CHAMY_API.Models.DTO
{
    public class HistoryDTO
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt {  get; set; }
    }
}
