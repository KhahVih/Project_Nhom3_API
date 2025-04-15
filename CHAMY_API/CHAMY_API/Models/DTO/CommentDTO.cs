namespace CHAMY_API.Models.DTO
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? Vote { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CustomerId { get; set; }
        public bool? IsShow { get; set; }
        public string CustomerName {  get; set; }
        public string ProductName { get; set; }

    }
}
