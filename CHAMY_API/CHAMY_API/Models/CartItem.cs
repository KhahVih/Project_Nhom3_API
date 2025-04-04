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
        public double FinalPrice
        {
            get
            {
                if (Product?.Sales != null && Product.Sales.IsActive &&
                    DateTime.Now >= Product.Sales.StartDate && DateTime.Now <= Product.Sales.EndDate)
                {
                    return UnitPrice * (1 - Product.Sales.DiscountPercentage / 100);
                }
                return UnitPrice;
            }
        }
        
    }
}
