namespace CHAMY_API.Models.DTO
{
    public class ShoppingCartDTO
    {
        public int Id { get; set; }
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CartItemDTO> CartItems { get; set; }
        public double TotalAmount => CartItems?.Sum(ci => ci.FinalPrice * ci.Quantity) ?? 0;
    }
}
