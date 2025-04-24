using System.ComponentModel.DataAnnotations;

namespace CHAMY_API.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string? PosCode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public double? Price { get; set; }
        public bool IsPublish { get; set; }
        public bool IsNew { get; set; }
        public int? Count { get; set; }

        // Mối quan hệ với Sale
        public int? SaleId { get; set; } // Khóa ngoại tới Sale (nullable nếu không có giảm giá)
        public Sale? Sales { get; set; } // Quan hệ 1-n với Sale

        // Thêm mối quan hệ n-n với Image
        public List<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public List<ProductCategory> ProductCategorys { get; set; } = new List<ProductCategory>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        // Mối quan hệ n-n với Color và Size
        public List<ProductColor>? ProductColors { get; set; } = new List<ProductColor>();
        public List<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
        public List<ProductVariant> ProductVariants { get; set; }
    }
}

