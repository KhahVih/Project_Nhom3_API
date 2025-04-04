namespace CHAMY_API.Models.DTO
{
    public class CheckOutDTO
    {
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string? Email { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Wards { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string? Note { get; set; }
        public List<CartItemDTO> CartItems { get; set; }
    }
}
