using System.ComponentModel.DataAnnotations.Schema;

namespace CHAMY_API.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? Vote { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CustomerId { get; set; }
        public bool? IsShow { get; set; }
        public Customer? Customer { get; set; }
        public Product Product { get; set; }
    }
}
