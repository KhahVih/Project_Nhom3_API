namespace CHAMY_API.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }
        public double UnitPrice { get; set; } // Giá gốc của sản phẩm
        public Product Product { get; set; }
        public Customer Customer { get; set; }
        public Color? Color { get; set; }
        public Size? Size { get; set; }

        // Tính giá sau khi áp dụng giảm giá (nếu có)
        public double FinalPrice { get; set;}
        
    }
}
