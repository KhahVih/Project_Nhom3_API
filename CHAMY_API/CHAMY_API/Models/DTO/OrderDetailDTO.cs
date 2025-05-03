namespace CHAMY_API.Models.DTO
{
    public class OrderDetailDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductPosCode { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double FinalPrice { get; set; }
        public int? ColorId { get; set; }
        public string ColorName { get; set; }
        public int? SizeId { get; set; }
        public string SizeName { get; set; }
    }
}
