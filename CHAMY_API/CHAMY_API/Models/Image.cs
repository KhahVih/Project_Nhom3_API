using System.ComponentModel.DataAnnotations;

namespace CHAMY_API.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Link { get; set; }

        // Thêm mối quan hệ n-n với Product
        public List<ProductImage> ProductImages { get; set; } 
    }
}