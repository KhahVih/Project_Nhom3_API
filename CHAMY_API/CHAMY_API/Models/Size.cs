namespace CHAMY_API.Models
{
    public class Size
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Mối quan hệ n-n với Product
        public List<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
    }
}
