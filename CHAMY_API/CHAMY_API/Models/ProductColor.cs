using System.ComponentModel.DataAnnotations;

namespace CHAMY_API.Models
{
    public class ProductColor
    {
        [Key]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Key]
        public int ColorId { get; set; }
        public Color Color { get; set; } = null!;
    }
}
