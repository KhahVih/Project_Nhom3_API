namespace CHAMY_API.Models.DTO
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProductCategoryDTO> ProductCategories { get; set; } = new List<ProductCategoryDTO>();
    }
}
