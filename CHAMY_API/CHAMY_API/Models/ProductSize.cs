using System.ComponentModel.DataAnnotations;

namespace CHAMY_API.Models
{
    public class ProductSize
    {
        [Key]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Key]
        public int SizeId { get; set; }
        public Size Size { get; set; } = null!;
    }
}
