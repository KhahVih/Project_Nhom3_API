namespace CHAMY_API.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ColorId { get; set; }
        public int SizeId { get; set; }
        public int Quantity { get; set; }

        public Product Product { get; set; }
        public Color Color { get; set; }
        public Size Size { get; set; }
    }
}
