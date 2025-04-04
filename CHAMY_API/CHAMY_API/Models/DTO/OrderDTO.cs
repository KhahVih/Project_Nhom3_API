namespace CHAMY_API.Models.DTO
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public double TotalAmount { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string? Phone { get; set; }
        public List<OrderDetailDTO> OrderItems { get; set; }
    }
}
