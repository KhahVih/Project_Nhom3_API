using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHAMY_API.Data;
using CHAMY_API.Models;
using CHAMY_API.Models.DTO;
namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            var categories = await _context.Category
                .Include(c => c.ProductCategory)
                .ThenInclude(pc => pc.Product)
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCategories = c.ProductCategory.Select(pc => new ProductCategoryDTO
                    {
                        ProductId = pc.ProductId,
                        CategoryId = pc.CategoryId,
                        ProductName = pc.Product != null ? pc.Product.Name : null,
                        CategoryName = pc.Category != null ? pc.Category.Name : null
                    }).ToList()
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var category = await _context.Category
                .Include(c => c.ProductCategory)
                .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var categoryDTO = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                ProductCategories = category.ProductCategory.Select(pc => new ProductCategoryDTO
                {
                    ProductId = pc.ProductId,
                    CategoryId = pc.CategoryId,
                    ProductName = pc.Product != null ? pc.Product.Name : null,
                    CategoryName = pc.Category != null ? pc.Category.Name : null
                }).ToList()
            };

            return Ok(categoryDTO);
        }

        // POST: api/Category
        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> CreateCategory(CategoryDTO categoryDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = new Category
            {
                Name = categoryDTO.Name,
                //ProductCategory = new List<ProductCategory>()
            };

            // Thêm các ProductCategory nếu có
            //if (categoryDTO.ProductCategories != null && categoryDTO.ProductCategories.Any())
            //{
            //    foreach (var pcDTO in categoryDTO.ProductCategories)
            //    {
            //        var product = await _context.Products.FindAsync(pcDTO.ProductId);
            //        if (product != null)
            //        {
            //            category.ProductCategory.Add(new ProductCategory
            //            {
            //                ProductId = pcDTO.ProductId,
            //                CategoryId = category.Id // Sẽ được cập nhật sau khi lưu
            //            });
            //        }
            //    }
            //}

            _context.Category.Add(category);
            await _context.SaveChangesAsync();

            // Cập nhật CategoryId trong ProductCategory sau khi category có Id
            foreach (var pc in category.ProductCategory)
            {
                pc.CategoryId = category.Id;
            }
            await _context.SaveChangesAsync();

            // Trả về DTO với thông tin đã tạo
            categoryDTO.Id = category.Id;
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDTO);
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDTO categoryDTO)
        {
            if (id != categoryDTO.Id)
            {
                return BadRequest("Category ID mismatch");
            }

            var category = await _context.Category
                .Include(c => c.ProductCategory)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin cơ bản
            category.Name = categoryDTO.Name;

            // Xóa các ProductCategory cũ
            _context.ProductCategory.RemoveRange(category.ProductCategory);
            category.ProductCategory.Clear();

            // Thêm lại ProductCategory từ DTO
            if (categoryDTO.ProductCategories != null && categoryDTO.ProductCategories.Any())
            {
                foreach (var pcDTO in categoryDTO.ProductCategories)
                {
                    var product = await _context.Products.FindAsync(pcDTO.ProductId);
                    if (product != null)
                    {
                        category.ProductCategory.Add(new ProductCategory
                        {
                            ProductId = pcDTO.ProductId,
                            CategoryId = category.Id
                        });
                    }
                }
            }

            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Category
                .Include(c => c.ProductCategory)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // Xóa các ProductCategory liên quan trước
            _context.ProductCategory.RemoveRange(category.ProductCategory);
            _context.Category.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}