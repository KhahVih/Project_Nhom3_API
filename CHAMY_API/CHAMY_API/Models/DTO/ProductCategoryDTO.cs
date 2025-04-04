namespace CHAMY_API.Models.DTO
{
    public class ProductCategoryDTO
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }

        // Thông tin cơ bản về Product (tùy chọn, có thể bỏ nếu không cần)
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
    }
}
