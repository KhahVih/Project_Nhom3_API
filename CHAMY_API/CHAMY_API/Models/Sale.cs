namespace CHAMY_API.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public string Name { get; set; } // Tên chương trình giảm giá (VD: "Summer Sale")
        public double DiscountPercentage { get; set; } // Phần trăm giảm giá (VD: 10.5 cho 10.5%)
        public DateTime StartDate { get; set; } // Ngày bắt đầu chương trình
        public DateTime EndDate { get; set; } // Ngày kết thúc chương trình
        public bool IsActive { get; set; } // Trạng thái hoạt động

        // Mối quan hệ 1-n với Product
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
