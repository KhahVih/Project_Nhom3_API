namespace CHAMY_API.Models
{
    public class Color
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Mối quan hệ n-n với Product
        public List<ProductColor> ProductColors { get; set; } = new List<ProductColor>();
    }
}
