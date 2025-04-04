namespace CHAMY_API.Models
{
    public class History
    {
        public int Id { get; set; }
        public int CustomerId { get; set; } // Khóa ngoại liên kết với Customer
        public string Action { get; set; } // Hành động (ví dụ: "Đăng nhập", "Đặt hàng", "Cập nhật thông tin")
        public string Description { get; set; } // Mô tả chi tiết
        public DateTime CreatedAt { get; set; } // Thời gian thực hiện hành động
        public string IpAddress { get; set; } // Địa chỉ IP (tùy chọn)

        // Quan hệ với Customer
        public Customer Customer { get; set; } // Navigation property
    }
}
