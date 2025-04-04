using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CHAMY_API.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Chờ xử lý")]
        Pending,
        [Display(Name = "Đang xử lý")]
        Processing,
        [Display(Name = "Đang giao hàng")]
        Shipped,
        [Display(Name = "Đã giao hàng")]
        Delivered,
        [Display(Name = "Đã hủy")]
        Cancelled

    }

    public class Order
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; } // Nullable cho khách chưa đăng nhập
        public string CustomerName { get; set; }// cho khách hàng chưa đăng nhập
        public string? Email { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Wards { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Note { get; set; }
        public double TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public Customer Customer { get; set; } // Quan hệ 1-1 với Customer, nullable
        public List<OrderDetail> OrderItems { get; set; } = new List<OrderDetail>();

    }
}