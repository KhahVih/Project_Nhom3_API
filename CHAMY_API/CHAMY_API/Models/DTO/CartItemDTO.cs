using CHAMY_API.DTOs;

namespace CHAMY_API.Models.DTO
{
    public class CartItemDTO
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public int? SizeId { get; set; }
        public string? SizeName { get; set; }
        public double UnitPrice { get; set; }
        public double FinalPrice { get; set; }
    }
}
