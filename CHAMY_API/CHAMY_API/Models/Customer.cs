namespace CHAMY_API.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Fullname { get; set; }
        public string Address { get; set; }
        public string? Phone { get; set; }
        public bool Gender { get; set; }
        public bool? IsClone { get; set; }
        public DateTime? CreatedAt { get; set; } 
        public DateTime? Date { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        // Thêm quan hệ với Order
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        // Thêm quan hệ với History
        public List<History> History { get; set; } = new List<History>();
    }
}
