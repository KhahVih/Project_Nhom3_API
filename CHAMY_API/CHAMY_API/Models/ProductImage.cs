using System.ComponentModel.DataAnnotations;

namespace CHAMY_API.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int ImageId { get; set; }
        public Image Image { get; set; }
    }
}