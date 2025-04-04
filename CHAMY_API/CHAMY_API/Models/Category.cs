using System.ComponentModel.DataAnnotations;

namespace CHAMY_API.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProductCategory> ProductCategory { get; set; } = new List<ProductCategory>();
    }
}
