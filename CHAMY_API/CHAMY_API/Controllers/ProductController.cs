using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHAMY_API.Models;
using CHAMY_API.DTOs;
using CHAMY_API.Data;
using CHAMY_API.Models.DTO;
using System.Text.Json;
using System.Drawing;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // láy danh sách tất cả sản phẩm
        [HttpGet("GetAllProduct")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts(int page = 1)
        {   
            const int pageSize = 21;
            if(page < 1) page = 1;
            // tổng sản phẩm
            var totalProduct = await _context.Products.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            // lấy danh sách sản phẩm 
            var products = await _context.Products
                .Skip(skip)
                .Take(pageSize)
             .Include(p => p.ProductImages)
             .ThenInclude(pi => pi.Image)
             .Include(p => p.Sales)
             .Include(p => p.ProductCategorys) // Thêm include cho ProductCategorys
             .ThenInclude(pc => pc.Category)   // Include Category từ ProductCategory
             .Select(p => new ProductDTO
             {
                 Id = p.Id,
                 PosCode = p.PosCode,
                 Name = p.Name,
                 Description = p.Description,
                 CreatedAt = p.CreatedAt,
                 UpdatedAt = p.UpdatedAt,
                 Price = p.Price,
                 IsPublish = p.IsPublish,
                 IsNew = p.IsNew,
                 SaleId = p.SaleId,
                 SaleName = p.Sales.Name,
                 DiscountPercentage = p.Sales.DiscountPercentage,
                 Count = p.Count,
                 Images = p.ProductImages.Select(pi => new ImageDTO
                 {
                     Id = pi.Image.Id,
                     Name = pi.Image.Name,
                     Link = pi.Image.Link
                 }).ToList(),
                 ProductCategorys = p.ProductCategorys.Select(pc => new ProductCategoryDTO
                 {
                     ProductId = pc.ProductId,
                     CategoryId = pc.CategoryId,
                     ProductName = pc.Product != null ? pc.Product.Name : null,
                     CategoryName = pc.Category != null ? pc.Category.Name : null, // Thêm tên danh mục
                 }).ToList(),
                 Colors = p.ProductColors.Select(pc => new ColorDTO
                 {
                     Id = pc.Color.Id,
                     Name = pc.Color.Name
                 }).ToList(),

                 // ✅ Thêm danh sách kích thước
                 Sizes = p.ProductSizes.Select(ps => new SizeDTO
                 {
                     Id = ps.Size.Id,
                     Name = ps.Size.Name
                 }).ToList()

             }).ToListAsync();
            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalProduct = totalProduct, // Tổng số sản phẩm 
                Products = products // Danh sách sản phẩm 
            };

            return Ok(result);
        }

        // Lấy danh sách tất các sản phẩm giảm giá 
        [HttpGet("GetDiscountedProducts")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetDiscountedProducts(int page = 1)
        {
            const int pageSize = 21;
            if (page < 1) page = 1;

            // Lọc sản phẩm có giảm giá
            var discountedQuery = _context.Products
                .Where(p => p.SaleId != null && p.Sales.DiscountPercentage > 0)
                .Include(p => p.ProductImages)
                    .ThenInclude(pi => pi.Image)
                .Include(p => p.Sales)
                .Include(p => p.ProductCategorys)
                    .ThenInclude(pc => pc.Category);

            var totalProduct = await discountedQuery.CountAsync();
            var totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);
            var skip = (page - 1) * pageSize;

            var products = await discountedQuery
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    PosCode = p.PosCode,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Price = p.Price,
                    IsPublish = p.IsPublish,
                    IsNew = p.IsNew,
                    SaleId = p.SaleId,
                    SaleName = p.Sales.Name,
                    DiscountPercentage = p.Sales.DiscountPercentage,
                    Count = p.Count,
                    Images = p.ProductImages.Select(pi => new ImageDTO
                    {
                        Id = pi.Image.Id,
                        Name = pi.Image.Name,
                        Link = pi.Image.Link
                    }).ToList(),
                    ProductCategorys = p.ProductCategorys.Select(pc => new ProductCategoryDTO
                    {
                        ProductId = pc.ProductId,
                        CategoryId = pc.CategoryId,
                        ProductName = pc.Product != null ? pc.Product.Name : null,
                        CategoryName = pc.Category != null ? pc.Category.Name : null,
                    }).ToList(),
                }).ToListAsync();

            var result = new
            {
                CurrentPage = page,
                TotalPages = totalPage,
                TotalProduct = totalProduct,
                Products = products
            };

            return Ok(result);
        }

        // Lấy danh sách sản phẩm đã xuất 
        [HttpGet("GetProductIsPuslish")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsIsPuslish(int page = 1)
        {
            const int pageSize = 21;
            if (page < 1) page = 1;
            var query = _context.Products
                .Where(p => p.IsPublish == true)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Sales)
                .Include(p => p.ProductCategorys)
                .ThenInclude(pc => pc.Category);
            // tổng sản phẩm
            var totalProduct = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            // lấy danh sách sản phẩm 
            var products = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new ProductDTO
                {
                     Id = p.Id,
                     PosCode = p.PosCode,
                     Name = p.Name,
                     Description = p.Description,
                     CreatedAt = p.CreatedAt,
                     UpdatedAt = p.UpdatedAt,
                     Price = p.Price,
                     IsPublish = p.IsPublish,
                     IsNew = p.IsNew,
                     SaleId = p.SaleId,
                     SaleName = p.Sales.Name,
                     Count = p.Count,
                     Images = p.ProductImages.Select(pi => new ImageDTO
                     {
                         Id = pi.Image.Id,
                         Name = pi.Image.Name,
                         Link = pi.Image.Link
                     }).ToList(),
                     ProductCategorys = p.ProductCategorys.Select(pc => new ProductCategoryDTO
                     {
                         ProductId = pc.ProductId,
                         CategoryId = pc.CategoryId,
                         ProductName = pc.Product != null ? pc.Product.Name : null,
                         CategoryName = pc.Category != null ? pc.Category.Name : null, // Thêm tên danh mục
                     }).ToList(),
                }).ToListAsync();

            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalProduct = totalProduct, // Tổng số sản phẩm 
                Products = products // Danh sách sản phẩm 
            };

            return Ok(result);
        }

        // Lấy danh sách sản phẩm chưa xuất 
        [HttpGet("GetProductNoPuslish")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsNoPuslish(int page = 1)
        {
            const int pageSize = 21;
            if (page < 1) page = 1;
            var query = _context.Products
                .Where(p => p.IsPublish == false)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Sales)
                .Include(p => p.ProductCategorys)
                .ThenInclude(pc => pc.Category);
            // tổng sản phẩm
            var totalProduct = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            // lấy danh sách sản phẩm 
            var products = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    PosCode = p.PosCode,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Price = p.Price,
                    IsPublish = p.IsPublish,
                    IsNew = p.IsNew,
                    SaleId = p.SaleId,
                    SaleName = p.Sales.Name,
                    Count = p.Count,
                    Images = p.ProductImages.Select(pi => new ImageDTO
                    {
                        Id = pi.Image.Id,
                        Name = pi.Image.Name,
                        Link = pi.Image.Link
                    }).ToList(),
                    ProductCategorys = p.ProductCategorys.Select(pc => new ProductCategoryDTO
                    {
                        ProductId = pc.ProductId,
                        CategoryId = pc.CategoryId,
                        ProductName = pc.Product != null ? pc.Product.Name : null,
                        CategoryName = pc.Category != null ? pc.Category.Name : null, // Thêm tên danh mục
                    }).ToList(),
                 
                }).ToListAsync();

            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalProduct = totalProduct, // Tổng số sản phẩm 
                Products = products // Danh sách sản phẩm 
            };

            return Ok(result);
        }

        // Lấy danh sách sản phẩm theo sale Id 
        [HttpGet("GetProductSale/{id}")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductSaleId(int id, int page = 1)
        {
            const int pageSize = 21;
            if (page < 1) page = 1;
            var query = _context.Products
                .Where(p => p.SaleId == id)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Sales)
                .Include(p => p.ProductCategorys)
                .ThenInclude(pc => pc.Category);   
            // tổng sản phẩm
            var totalProduct = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            // lấy danh sách sản phẩm 
            var products = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new ProductDTO
                {
                     Id = p.Id,
                     PosCode = p.PosCode,
                     Name = p.Name,
                     Description = p.Description,
                     CreatedAt = p.CreatedAt,
                     UpdatedAt = p.UpdatedAt,
                     Price = p.Price,
                     IsPublish = p.IsPublish,
                     IsNew = p.IsNew,
                     SaleId = p.SaleId,
                     SaleName = p.Sales.Name,
                     Count = p.Count,
                     Images = p.ProductImages.Select(pi => new ImageDTO
                     {
                         Id = pi.Image.Id,
                         Name = pi.Image.Name,
                         Link = pi.Image.Link
                     }).ToList(),
                     ProductCategorys = p.ProductCategorys.Select(pc => new ProductCategoryDTO
                     {
                         ProductId = pc.ProductId,
                         CategoryId = pc.CategoryId,
                         ProductName = pc.Product != null ? pc.Product.Name : null,
                         CategoryName = pc.Category != null ? pc.Category.Name : null, // Thêm tên danh mục
                     }).ToList(),
                    
                }).ToListAsync();

            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalProduct = totalProduct, // Tổng số sản phẩm 
                Products = products // Danh sách sản phẩm 
            };

            return Ok(result);
        }

        // Lấy danh sách sản phẩm mới 
        [HttpGet("GetProductNew")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsNew(int page =1)
        {
            const int pageSize = 21;
            if (page < 1) page = 1;
            var query = _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Sales)
                .Include(p => p.ProductCategorys)
                .ThenInclude(pc => pc.Category);
            // tổng sản phẩm
            var totalProduct = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            // lấy danh sách sản phẩm 
            var products = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    PosCode = p.PosCode,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Price = p.Price,
                    IsPublish = p.IsPublish,
                    IsNew = p.IsNew,
                    SaleId = p.SaleId,
                    SaleName = p.Sales.Name,
                    Count = p.Count,
                    Images = p.ProductImages.Select(pi => new ImageDTO
                    {
                        Id = pi.Image.Id,
                        Name = pi.Image.Name,
                        Link = pi.Image.Link
                    }).ToList(),
                    ProductCategorys = p.ProductCategorys.Select(pc => new ProductCategoryDTO
                    {
                        ProductId = pc.ProductId,
                        CategoryId = pc.CategoryId,
                        ProductName = pc.Product != null ? pc.Product.Name : null,
                        CategoryName = pc.Category != null ? pc.Category.Name : null, // Thêm tên danh mục
                    }).ToList(),
                    
                }).ToListAsync();

            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalProduct = totalProduct, // Tổng số sản phẩm 
                Products = products // Danh sách sản phẩm 
            };

            return Ok(result);
        }

        // Lấy danh sách sản phẩm cũ 
        [HttpGet("GetProductOld")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsOld(int page = 1)
        {
            const int pageSize = 21;
            if (page < 1) page = 1;
            var query = _context.Products
                .OrderBy(p => p.CreatedAt)
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Sales)
                .Include(p => p.ProductCategorys)
                .ThenInclude(pc => pc.Category);
            // tổng sản phẩm
            var totalProduct = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            // lấy danh sách sản phẩm 
            var products = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    PosCode = p.PosCode,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Price = p.Price,
                    IsPublish = p.IsPublish,
                    IsNew = p.IsNew,
                    SaleId = p.SaleId,
                    SaleName = p.Sales.Name,
                    Count = p.Count,
                    Images = p.ProductImages.Select(pi => new ImageDTO
                    {
                        Id = pi.Image.Id,
                        Name = pi.Image.Name,
                        Link = pi.Image.Link
                    }).ToList(),
                    ProductCategorys = p.ProductCategorys.Select(pc => new ProductCategoryDTO
                    {
                        ProductId = pc.ProductId,
                        CategoryId = pc.CategoryId,
                        ProductName = pc.Product != null ? pc.Product.Name : null,
                        CategoryName = pc.Category != null ? pc.Category.Name : null, // Thêm tên danh mục
                    }).ToList(),
                    
                }).ToListAsync();

            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalProduct = totalProduct, // Tổng số sản phẩm 
                Products = products // Danh sách sản phẩm 
            };

            return Ok(result);
        }

        // Lấy danh sách sản phẩm có giá từ thấp đến cao 
        [HttpGet("GetProductPriceASC")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsPriceASC( int page = 1)
        {
            const int pageSize = 21;
            if (page < 1) page = 1;
            var query = _context.Products
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Sales)
                .Include(p => p.ProductCategorys)
                .ThenInclude(pc => pc.Category)   
                .OrderBy(p => p.Price);
            // tổng sản phẩm
            var totalProduct = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            // lấy danh sách sản phẩm 
            var products = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new ProductDTO
                {
                     Id = p.Id,
                     PosCode = p.PosCode,
                     Name = p.Name,
                     Description = p.Description,
                     CreatedAt = p.CreatedAt,
                     UpdatedAt = p.UpdatedAt,
                     Price = p.Price,
                     IsPublish = p.IsPublish,
                     IsNew = p.IsNew,
                     SaleId = p.SaleId,
                     SaleName = p.Sales.Name,
                     Count = p.Count,
                     Images = p.ProductImages.Select(pi => new ImageDTO
                     {
                         Id = pi.Image.Id,
                         Name = pi.Image.Name,
                         Link = pi.Image.Link
                     }).ToList(),
                     ProductCategorys = p.ProductCategorys.Select(pc => new ProductCategoryDTO
                     {
                         ProductId = pc.ProductId,
                         CategoryId = pc.CategoryId,
                         ProductName = pc.Product != null ? pc.Product.Name : null,
                         CategoryName = pc.Category != null ? pc.Category.Name : null, // Thêm tên danh mục
                     }).ToList(),
                    
                }).ToListAsync();

            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalProduct = totalProduct, // Tổng số sản phẩm 
                Products = products // Danh sách sản phẩm 
            };

            return Ok(result);
        }


        [HttpGet("GetProductPriceASDC")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsPriceASDC(int page = 1)
        {
            const int pageSize = 21;
            if (page < 1) page = 1;
            var query = _context.Products
                .Include(p => p.ProductImages)
                .ThenInclude(pi => pi.Image)
                .Include(p => p.Sales)
                .Include(p => p.ProductCategorys)
                .ThenInclude(pc => pc.Category)
                .OrderByDescending(p => p.Price);
            // tổng sản phẩm
            var totalProduct = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            // lấy danh sách sản phẩm 
            var products = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    PosCode = p.PosCode,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Price = p.Price,
                    IsPublish = p.IsPublish,
                    IsNew = p.IsNew,
                    SaleId = p.SaleId,
                    SaleName = p.Sales.Name,
                    Count = p.Count,
                    Images = p.ProductImages.Select(pi => new ImageDTO
                    {
                        Id = pi.Image.Id,
                        Name = pi.Image.Name,
                        Link = pi.Image.Link
                    }).ToList(),
                    ProductCategorys = p.ProductCategorys.Select(pc => new ProductCategoryDTO
                    {
                        ProductId = pc.ProductId,
                        CategoryId = pc.CategoryId,
                        ProductName = pc.Product != null ? pc.Product.Name : null,
                        CategoryName = pc.Category != null ? pc.Category.Name : null, // Thêm tên danh mục
                    }).ToList(),
                   
                }).ToListAsync();

            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalProduct = totalProduct, // Tổng số sản phẩm 
                Products = products // Danh sách sản phẩm 
            };

            return Ok(result);
        }

        // Lấy sản phẩm theo Id 
        [HttpGet("GetProduct/{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                    .ThenInclude(pi => pi.Image)
                .Include(p => p.Sales)
                .Include(p => p.ProductCategorys)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.ProductColors)
                    .ThenInclude(pc => pc.Color) // Include màu sắc
                .Include(p => p.ProductSizes)
                    .ThenInclude(ps => ps.Size)  // Include kích thước
                .Include(p => p.Comments) // Thêm dòng này để lấy Comments
                    .ThenInclude(c => c.Customer) // Nếu cần thông tin Customer (Fullname)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var productDTO = new ProductDTO
            {
                Id = product.Id,
                PosCode = product.PosCode,
                Name = product.Name,
                Description = product.Description,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Price = product.Price,
                IsPublish = product.IsPublish,
                IsNew = product.IsNew,
                SaleId = product.SaleId,
                SaleName = _context.Sale.FirstOrDefault(s => s.Id == product.SaleId)?.Name,
                DiscountPercentage = product.Sales?.DiscountPercentage,
                Count = product.Count,
                Images = product.ProductImages.Select(pi => new ImageDTO
                {
                    Id = pi.Image.Id,
                    Name = pi.Image.Name,
                    Link = pi.Image.Link
                }).ToList(),
                ProductCategorys = product.ProductCategorys.Select(pc => new ProductCategoryDTO
                {
                    ProductId = pc.ProductId,
                    CategoryId = pc.CategoryId,
                    ProductName = pc.Product != null ? pc.Product.Name : null,
                    CategoryName = pc.Category != null ? pc.Category.Name : null // Thêm tên danh mục
                }).ToList(),
                Comments = product.Comments.Select(c => new CommentDTO
                {
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    CustomerId = c.CustomerId,
                    CustomerName = c.Customer.Fullname,
                    Vote = c.Vote,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,

                }).ToList(),
                // ✅ Thêm danh sách màu sắc
                Colors = product.ProductColors.Select(pc => new ColorDTO
                {
                    Id = pc.Color.Id,
                    Name = pc.Color.Name
                }).ToList(),

                // ✅ Thêm danh sách kích thước
                Sizes = product.ProductSizes.Select(ps => new SizeDTO
                {
                    Id = ps.Size.Id,
                    Name = ps.Size.Name
                }).ToList()
            };

            return Ok(productDTO);
        }

        // Lấy danh sách sản phẩm theo danh mục 
        [HttpGet("GetProductCategory/{id}")]
        public async Task<ActionResult<ProductDTO>> GetProductByCategory(int id, int page = 1)
        {
            const int pageSize = 21;
            if (page < 1) page = 1;
            var query = _context.Products
                .Where(p => p.ProductCategorys.Any(pc => pc.CategoryId == id))
                .Include(p => p.ProductImages)
                    .ThenInclude(pi => pi.Image)
                .Include(p => p.Sales)
                .Include(p => p.ProductCategorys)
                    .ThenInclude(pc => pc.Category);
            // tổng sản phẩm
            var totalProduct = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            // lấy danh sách sản phẩm 
            var product = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            if (product == null || !product.Any())
            {
                return NotFound();
            }

            var productDTO = product.Select( product => new ProductDTO
            {
                Id = product.Id,
                PosCode = product.PosCode,
                Name = product.Name,
                Description = product.Description,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Price = product.Price,
                IsPublish = product.IsPublish,
                IsNew = product.IsNew,
                SaleId = product.SaleId,
                SaleName = product.Sales?.Name,
                Count = product.Count,
                Images = product.ProductImages != null ? product.ProductImages
                    .Where(pi => pi.Image != null)
                    .Select(pi => new ImageDTO
                    {
                        Id = pi.Image.Id,
                        Name = pi.Image.Name,
                        Link = pi.Image.Link,
                    }).ToList(): new List<ImageDTO>(),
                ProductCategorys = product.ProductCategorys.Select(pc => new ProductCategoryDTO
                {
                    ProductId = pc.ProductId,
                    CategoryId = pc.CategoryId,
                    ProductName = pc.Product != null ? pc.Product.Name : null,
                    CategoryName = pc.Category != null ? pc.Category.Name : null // Thêm tên danh mục
                }).ToList(),
            }).ToList();


            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalProduct = totalProduct, // Tổng số sản phẩm 
                Products = productDTO // Danh sách sản phẩm 
            };

            return Ok(result);
        }

        // Tìm kiếm sản phẩm theo tên hoặc theo PosCode 
        [HttpGet("SearchProduct/{name}")]
        public async Task<ActionResult<ProductDTO>> GetProductName(string name, int page = 1)
        {
            const int pageSize = 21;
            if (page < 1) page = 1;
            var query = _context.Products
                 .Include(p => p.ProductImages)
                 .ThenInclude(pi => pi.Image)
                 .Include(p => p.ProductCategorys)
                 .ThenInclude(pc => pc.Category)
                 .Include(p => p.Sales)
                 .Where(p => p.Name.Contains(name) || p.PosCode.Contains(name));
            // tổng sản phẩm
            var totalProduct = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalProduct / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            // lấy danh sách sản phẩm 
            var products = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            if (products == null)
            {
                return NotFound();
            }

            var productDTO = products.Select(product => new ProductDTO
            {
                Id = product.Id,
                PosCode = product.PosCode,
                Name = product.Name,
                Description = product.Description,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Price = product.Price,
                IsPublish = product.IsPublish,
                IsNew = product.IsNew,
                SaleId = product.SaleId,
                SaleName = product.Sales?.Name,
                Count = product.Count,
                Images = product.ProductImages != null ? product.ProductImages
                    .Where(pi => pi.Image != null)
                    .Select(pi => new ImageDTO
                    {
                        Id = pi.Image.Id,
                        Name = pi.Image.Name,
                        Link = pi.Image.Link,
                    }).ToList() : new List<ImageDTO>(),
                ProductCategorys = product.ProductCategorys.Select(pc => new ProductCategoryDTO
                {
                    ProductId = pc.ProductId,
                    CategoryId = pc.CategoryId,
                    ProductName = pc.Product != null ? pc.Product.Name : null,
                    CategoryName = pc.Category != null ? pc.Category.Name : null // Thêm tên danh mục
                }).ToList(),
            }).ToList();

            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalProduct = totalProduct, // Tổng số sản phẩm 
                Products = productDTO // Danh sách sản phẩm 
            };

            return Ok(result);
        }

        // thêm product
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> CreateProduct([FromForm] string productDTOJson, [FromForm] List<IFormFile> imageFiles)
        {
            if (string.IsNullOrEmpty(productDTOJson))
            {
                return BadRequest("Product data is required.");
            }

            // Parse JSON string thành ProductDTO
            ProductDTO productDTO;
            try
            {
                productDTO = JsonSerializer.Deserialize<ProductDTO>(productDTOJson);
            }
            catch (JsonException ex)
            {
                return BadRequest($"Invalid product data format: {ex.Message}");
            }

            // Tạo product mới 
            var product = new Product
            {
                PosCode = productDTO.PosCode,
                Name = productDTO.Name,
                Description = productDTO.Description,
                Price = productDTO.Price,
                IsPublish = productDTO.IsPublish,
                IsNew = productDTO.IsNew,
                Count = productDTO.Count,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ProductImages = new List<ProductImage>(),
                ProductCategorys = new List<ProductCategory>(),
                ProductColors = new List<ProductColor>(),
                ProductSizes = new List<ProductSize>(),
            };

            // Gán Sale nếu có
            if (productDTO.SaleId.HasValue)
            {
                var sale = await _context.Sale.FindAsync(productDTO.SaleId.Value);
                if (sale != null)
                {
                    product.SaleId = sale.Id;
                }
            }
            


            // Xử lý upload hình ảnh nếu có
            if (imageFiles != null && imageFiles.Any())
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var uploadFolder = Path.Combine(_environment.WebRootPath, "Image");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                foreach (var file in imageFiles)
                {
                    if (file.Length == 0)
                    {
                        continue;
                    }

                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        continue;
                    }

                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileUrl = $"{Request.Scheme}://{Request.Host}/Image/{fileName}";

                    var newImage = new Image
                    {
                        Link = fileUrl,
                        Name = file.FileName ?? "Unnamed",
                        ProductImages = new List<ProductImage>()
                    };
                    _context.Images.Add(newImage);
                    await _context.SaveChangesAsync();

                    var productImage = new ProductImage
                    {
                        ProductId = product.Id,
                        ImageId = newImage.Id,
                        Product = product,
                        Image = newImage
                    };
                    product.ProductImages.Add(productImage);
                }
            }

            // Xử lý danh sách danh mục
            if (productDTO.ProductCategorys != null && productDTO.ProductCategorys.Any())
            {
                foreach (var pcDTO in productDTO.ProductCategorys)
                {
                    var existingCategory = await _context.Category
                        .FirstOrDefaultAsync(c => c.Id == pcDTO.CategoryId);

                    if (existingCategory != null)
                    {
                        var productCategory = new ProductCategory
                        {
                            Product = product,
                            Category = existingCategory,
                            CategoryId = existingCategory.Id
                        };
                        product.ProductCategorys.Add(productCategory);
                    }
                }
            }
            // Xử lý màu sắc
            if (productDTO.Colors != null && productDTO.Colors.Any())
            {
                foreach (var colorDTO in productDTO.Colors)
                {
                    var color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == colorDTO.Id);
                    if (color != null)
                    {
                        var productcolor = new ProductColor
                        {
                            Product = product,
                            Color = color,
                            ColorId = color.Id
                        };
                        product.ProductColors.Add(productcolor);
                    }
                }
            }
            // Xử lý kích thước
            if (productDTO.Sizes != null && productDTO.Sizes.Any())
            {
                foreach (var sizeDTO in productDTO.Sizes)
                {
                    var size = await _context.Sizes.FindAsync(sizeDTO.Id);
                    if (size != null)
                    {
                        var productsize = new ProductSize
                        {
                            Product = product,
                            Size = size,
                            SizeId = size.Id
                        };
                        product.ProductSizes.Add(productsize);
                    }
                }
            }
            // Sau khi SaveChanges để có product.Id
            //if (productDTO.Variants != null && productDTO.Variants.Any())
            //{
            //    foreach (var variantDTO in productDTO.Variants)
            //    {
            //        var productVariant = new ProductVariant
            //        {
            //            ProductId = product.Id,
            //            ColorId = variantDTO.ColorId,
            //            SizeId = variantDTO.SizeId,
            //            Quantity = variantDTO.Quantity
            //        };
            //        _context.ProductVariants.Add(productVariant);
            //    }
            //    await _context.SaveChangesAsync();
            //}

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            foreach (var productImage in product.ProductImages)
            {
                productImage.ProductId = product.Id;
            }
            // Tạo các bản ghi ProductVariant
            //foreach (var variant in product.ProductVariants)
            //{
            //    var pv = new ProductVariant
            //    {
            //        ProductId = product.Id,
            //        ColorId = variant.ColorId,
            //        SizeId = variant.SizeId,
            //        Quantity = variant.Quantity
            //    };
            //}
            //await _context.SaveChangesAsync();

            productDTO.Id = product.Id;
            productDTO.ProductCategorys = product.ProductCategorys.Select(pc => new ProductCategoryDTO
            {
                ProductId = pc.ProductId,
                CategoryId = pc.CategoryId,
                CategoryName = pc.Category.Name,
            }).ToList();

            productDTO.Images = product.ProductImages.Select(pi => new ImageDTO
            {
                Id = pi.Image.Id,
                Link = pi.Image.Link,
                Name = pi.Image.Name
            }).ToList();
            productDTO.Colors = product.ProductColors.Select(pc => new ColorDTO
            {
                Id = pc.Color.Id,
                Name = pc.Color.Name
            }).ToList();

            productDTO.Sizes = product.ProductSizes.Select(ps => new SizeDTO
            {
                Id = ps.Size.Id,
                Name = ps.Size.Name
            }).ToList();
            // Nếu có Sale thì trả về luôn trong DTO
            if (product.Sales != null)
            {
                productDTO.Sale = new SaleDTO
                {
                    Id = product.Sales.Id,
                    Name = product.Sales.Name,
                    DiscountPercentage = product.Sales.DiscountPercentage,
                    StartDate = product.Sales.StartDate,
                    EndDate = product.Sales.EndDate
                };
            }


            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDTO);
        }

        // PUT: api/products/{id}
        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] string productDTOJson, [FromForm] List<IFormFile> imageFiles)
        {
            if (string.IsNullOrEmpty(productDTOJson))
            {
                return BadRequest("Product data is required.");
            }

            // Parse JSON string thành ProductDTO
            ProductDTO productDTO;
            try
            {
                productDTO = JsonSerializer.Deserialize<ProductDTO>(productDTOJson);
            }
            catch (JsonException ex)
            {
                return BadRequest($"Invalid product data format: {ex.Message}");
            }

            if (id != productDTO.Id)
            {
                return BadRequest("Product ID mismatch.");
            }

            var product = await _context.Products
                .Include(p => p.ProductImages)
                    .ThenInclude(pi => pi.Image)
                .Include(p => p.ProductCategorys)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin sản phẩm
            product.PosCode = productDTO.PosCode;
            product.Name = productDTO.Name;
            product.Description = productDTO.Description;
            product.Price = productDTO.Price;
            product.IsPublish = productDTO.IsPublish;
            product.IsNew = productDTO.IsNew;
            product.Count = productDTO.Count;
            product.UpdatedAt = DateTime.UtcNow;
            // **Cập nhật SaleId**
            product.SaleId = productDTO.SaleId;

            // **Cập nhật danh sách ảnh**
            // Lấy danh sách ImageId hiện có
            var existingImageIds = product.ProductImages.Select(pi => pi.ImageId).ToList();
            // Lấy danh sách ImageId từ DTO (xử lý trường hợp null)
            var dtoImageIds = productDTO.Images?.Select(img => img.Id).ToList() ?? new List<int>();
            // Xác định các ImageId cần xóa
            var toRemoveImageIds = existingImageIds.Except(dtoImageIds).ToList();
            // Xác định các ImageId cần giữ lại hoặc thêm từ DTO
            var toKeepImageIds = dtoImageIds.Where(id => existingImageIds.Contains(id)).ToList();

            // Xóa các ProductImage không còn trong DTO
            foreach (var imageId in toRemoveImageIds)
            {
                var piToRemove = product.ProductImages.FirstOrDefault(pi => pi.ImageId == imageId);
                if (piToRemove != null)
                {
                    product.ProductImages.Remove(piToRemove);
                    // Xóa ảnh khỏi bảng Image nếu không còn liên kết nào khác
                    var image = await _context.Images
                        .Include(i => i.ProductImages)
                        .FirstOrDefaultAsync(i => i.Id == imageId);
                    if (image != null && !image.ProductImages.Any())
                    {
                        _context.Images.Remove(image);
                    }
                }
            }

            // Thêm các ảnh mới từ DTO (nếu có ImageId không tồn tại trong danh sách hiện tại)
            foreach (var imageId in dtoImageIds.Except(existingImageIds))
            {
                var existingImage = await _context.Images.FindAsync(imageId);
                if (existingImage != null)
                {
                    product.ProductImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageId = imageId,
                        Product = product,
                        Image = existingImage
                    });
                }
            }

            // Xử lý upload ảnh mới từ imageFiles
            if (imageFiles != null && imageFiles.Any())
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var uploadFolder = Path.Combine(_environment.WebRootPath, "Image");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                foreach (var file in imageFiles)
                {
                    if (file.Length == 0) continue;

                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension)) continue;

                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileUrl = $"{Request.Scheme}://{Request.Host}/Image/{fileName}";

                    var newImage = new Image
                    {
                        Link = fileUrl,
                        Name = file.FileName ?? "Unnamed",
                        ProductImages = new List<ProductImage>()
                    };
                    _context.Images.Add(newImage);
                    await _context.SaveChangesAsync();

                    var productImage = new ProductImage
                    {
                        ProductId = product.Id,
                        ImageId = newImage.Id,
                        Product = product,
                        Image = newImage
                    };
                    product.ProductImages.Add(productImage);
                }
            }

            // **Cập nhật danh sách danh mục**
            var existingCategoryIds = product.ProductCategorys.Select(pc => pc.CategoryId).ToList();
            var dtoCategoryIds = productDTO.ProductCategorys?.Select(pc => pc.CategoryId).ToList() ?? new List<int>();
            var toRemoveCategoryIds = existingCategoryIds.Except(dtoCategoryIds).ToList();
            var toAddCategoryIds = dtoCategoryIds.Except(existingCategoryIds).ToList();

            foreach (var categoryId in toRemoveCategoryIds)
            {
                var pcToRemove = product.ProductCategorys.FirstOrDefault(pc => pc.CategoryId == categoryId);
                if (pcToRemove != null)
                {
                    product.ProductCategorys.Remove(pcToRemove);
                }
            }

            foreach (var categoryId in toAddCategoryIds)
            {
                var existingCategory = await _context.Category.FindAsync(categoryId);
                if (existingCategory != null)
                {
                    product.ProductCategorys.Add(new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId,
                        Product = product,
                        Category = existingCategory
                    });
                }
            }
            // === Cập nhật danh sách màu sắc ===
            var existingColorIds = await _context.ProductColors
                .Where(pc => pc.ProductId == product.Id)
                .Select(pc => pc.ColorId)
                .ToListAsync();

            var dtoColorIds = productDTO.Colors?.Select(c => c.Id).ToList() ?? new List<int>();

            var toRemoveColorIds = existingColorIds.Except(dtoColorIds).ToList();
            var toAddColorIds = dtoColorIds.Except(existingColorIds).ToList();

            // Xóa màu không còn được chọn
            foreach (var colorId in toRemoveColorIds)
            {
                var pcToRemove = await _context.ProductColors
                    .FirstOrDefaultAsync(pc => pc.ProductId == product.Id && pc.ColorId == colorId);
                if (pcToRemove != null)
                {
                    _context.ProductColors.Remove(pcToRemove);
                }
            }

            // Thêm màu mới
            foreach (var colorId in toAddColorIds)
            {
                var color = await _context.Colors.FindAsync(colorId);
                if (color != null)
                {
                    _context.ProductColors.Add(new ProductColor
                    {
                        ProductId = product.Id,
                        ColorId = color.Id
                    });
                }
            }

            // === Cập nhật danh sách kích cỡ ===
            var existingSizeIds = await _context.ProductSizes
                .Where(ps => ps.ProductId == product.Id)
                .Select(ps => ps.SizeId)
                .ToListAsync();

            var dtoSizeIds = productDTO.Sizes?.Select(s => s.Id).ToList() ?? new List<int>();

            var toRemoveSizeIds = existingSizeIds.Except(dtoSizeIds).ToList();
            var toAddSizeIds = dtoSizeIds.Except(existingSizeIds).ToList();

            // Xóa size không còn được chọn
            foreach (var sizeId in toRemoveSizeIds)
            {
                var psToRemove = await _context.ProductSizes
                    .FirstOrDefaultAsync(ps => ps.ProductId == product.Id && ps.SizeId == sizeId);
                if (psToRemove != null)
                {
                    _context.ProductSizes.Remove(psToRemove);
                }
            }

            // Thêm size mới
            foreach (var sizeId in toAddSizeIds)
            {
                var size = await _context.Sizes.FindAsync(sizeId);
                if (size != null)
                {
                    _context.ProductSizes.Add(new ProductSize
                    {
                        ProductId = product.Id,
                        SizeId = size.Id
                    });
                }
            }

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Cập nhật productDTO để trả về (nếu muốn)
            productDTO.Images = product.ProductImages.Select(pi => new ImageDTO
            {
                Id = pi.Image.Id,
                Link = pi.Image.Link,
                Name = pi.Image.Name
            }).ToList();

            productDTO.ProductCategorys = product.ProductCategorys.Select(pc => new ProductCategoryDTO
            {
                ProductId = pc.ProductId,
                CategoryId = pc.CategoryId,
                CategoryName = pc.Category.Name 
            }).ToList();
            productDTO.Colors = await _context.ProductColors
                .Where(pc => pc.ProductId == product.Id)
                .Select(pc => new ColorDTO
                {
                    Id = pc.Color.Id,
                    Name = pc.Color.Name
                })
                .ToListAsync();
            productDTO.Sizes = await _context.ProductSizes
            .Where(ps => ps.ProductId == product.Id)
            .Select(ps => new SizeDTO
            {
                Id = ps.Size.Id,
                Name = ps.Size.Name
            })
            .ToListAsync();

            return Ok(productDTO); // Trả về productDTO thay vì NoContent để frontend nhận dữ liệu mới
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // Get Color
        [HttpGet("GetColor")]
        public async Task<ActionResult<IEnumerable<ColorDTO>>> GetColor()
        {
            var color = await _context.Colors
                .ToListAsync();


            return Ok(color);
        }
        // Get Size
        [HttpGet("GetSize")]
        public async Task<ActionResult<IEnumerable<ColorDTO>>> GetSize()
        {
            var size = await _context.Sizes
                .ToListAsync();


            return Ok(size);
        }

        // GET: api/products/{id}/variants
        [HttpGet("{id}/variants")]
        public async Task<IActionResult> GetVariants(int id)
        {
            var variants = await _context.ProductVariants
                .Where(v => v.ProductId == id)
                .Include(v => v.Color)
                .Include(v => v.Size)
                .Select(v => new {
                    ColorId = v.ColorId,
                    ColorName = v.Color.Name,
                    SizeId = v.SizeId,
                    SizeName = v.Size.Name,
                    Quantity = v.Quantity
                })
                .ToListAsync();

            return Ok(variants);
        }

        // POST: api/products/{id}/add-to-cart
        [HttpPost("{id}/add-to-cart")]
        public async Task<IActionResult> AddToCart(int id, [FromBody] VariantDto dto)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.ProductId == id && v.ColorId == dto.ColorId && v.SizeId == dto.SizeId);

            if (variant == null)
            {
                return NotFound("Variant not found");
            }

            if (variant.Quantity <= 0)
            {
                return BadRequest("This variant is out of stock");
            }

            // TODO: Add to cart logic here
            return Ok("Item added to cart");
        }

        // PUT: api/products/{id}/variants/update-stock
        [HttpPut("{id}/variants/update-stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] List<VariantDto> variants)
        {
            foreach (var dto in variants)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.ProductId == id && v.ColorId == dto.ColorId && v.SizeId == dto.SizeId);

                if (variant != null)
                {
                    variant.Quantity = dto.Quantity;
                }
                else
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = id,
                        ColorId = dto.ColorId,
                        SizeId = dto.SizeId,
                        Quantity = dto.Quantity
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Stock updated");
        }

    }

}