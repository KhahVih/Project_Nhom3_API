using CHAMY_API.Models.DTO;

namespace CHAMY_API.DTOs
{
    public class ProductDTO
    {
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
        public int? SaleId { get; set; }
        public string? SaleName { get; set; }
        public double? DiscountPercentage { get; set; }
        public List<ImageDTO>? Images { get; set; } // Danh sách ảnh với Link
        public List<ProductCategoryDTO>? ProductCategorys { get; set; } // Sản phẩm thuộc danh mục 
        public List<CommentDTO>? Comments { get; set; } = new List<CommentDTO>();
    }
}