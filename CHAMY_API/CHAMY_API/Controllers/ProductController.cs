using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHAMY_API.Models;
using CHAMY_API.DTOs;
using CHAMY_API.Data;
using CHAMY_API.Models.DTO;
using System.Text.Json;

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
                 Comments = p.Comments.Select(c => new CommentDTO
                 {
                     ProductId= c.ProductId,
                     ProductName = c.product.Name,
                     CustomerId = c.CustomerId,
                     CustomerName = c.customer.Fullname,
                     Vote = c.Vote,
                     Description = c.Description,
                     CreatedAt = c.CreatedAt,
                     
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
                     Comments = p.Comments.Select(c => new CommentDTO
                     {
                         ProductId = c.ProductId,
                         ProductName = c.product.Name,
                         CustomerId = c.CustomerId,
                         CustomerName = c.customer.Fullname,
                         Vote = c.Vote,
                         Description = c.Description,

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
                    Comments = p.Comments.Select(c => new CommentDTO
                    {
                        ProductId = c.ProductId,
                        ProductName = c.product.Name,
                        CustomerId = c.CustomerId,
                        CustomerName = c.customer.Fullname,
                        Vote = c.Vote,
                        Description = c.Description,

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
                     Comments = p.Comments.Select(c => new CommentDTO
                     {
                         ProductId = c.ProductId,
                         ProductName = c.product.Name,
                         CustomerId = c.CustomerId,
                         CustomerName = c.customer.Fullname,
                         Vote = c.Vote,
                         Description = c.Description,

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
                    Comments = p.Comments.Select(c => new CommentDTO
                    {
                        ProductId = c.ProductId,
                        ProductName = c.product.Name,
                        CustomerId = c.CustomerId,
                        CustomerName = c.customer.Fullname,
                        Vote = c.Vote,
                        Description = c.Description,

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
                    Comments = p.Comments.Select(c => new CommentDTO
                    {
                        ProductId = c.ProductId,
                        ProductName = c.product.Name,
                        CustomerId = c.CustomerId,
                        CustomerName = c.customer.Fullname,
                        Vote = c.Vote,
                        Description = c.Description,

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
                    Comments = p.Comments.Select(c => new CommentDTO
                    {
                        ProductId = c.ProductId,
                        ProductName = c.product.Name,
                        CustomerId = c.CustomerId,
                        CustomerName = c.customer.Fullname,
                        Vote = c.Vote,
                        Description = c.Description,

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
                    Comments = p.Comments.Select(c => new CommentDTO
                    {
                        ProductId = c.ProductId,
                        ProductName = c.product.Name,
                        CustomerId = c.CustomerId,
                        CustomerName = c.customer.Fullname,
                        Vote = c.Vote,
                        Description = c.Description,

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
                .Include(p => p.Comments) // Thêm dòng này để lấy Comments
                    .ThenInclude(c => c.customer) // Nếu cần thông tin Customer (Fullname)
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
                    ProductName = c.product.Name,
                    CustomerId = c.CustomerId,
                    CustomerName = c.customer.Fullname,
                    Vote = c.Vote,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,

                }).ToList(),
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
                Comments = product.Comments.Select(c => new CommentDTO
                {
                    ProductId = c.ProductId,
                    ProductName = c.product.Name,
                    CustomerId = c.CustomerId,
                    CustomerName = c.customer.Fullname,
                    Vote = c.Vote,
                    Description = c.Description,

                }).ToList()
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
                Comments = product.Comments.Select(c => new CommentDTO
                {
                    ProductId = c.ProductId,
                    ProductName = c.product.Name,
                    CustomerId = c.CustomerId,
                    CustomerName = c.customer.Fullname,
                    Vote = c.Vote,
                    Description = c.Description,

                }).ToList()
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

        // POST: api/products
        //[HttpPost]
        //public async Task<ActionResult<ProductDTO>> CreateProduct(ProductDTO productDTO)
        //{
        //    if (productDTO == null)
        //    {
        //        return BadRequest("Product data is required.");
        //    }

        //    // Tạo product mới 
        //    var product = new Product
        //    {
        //        PosCode = productDTO.PosCode,
        //        Name = productDTO.Name,
        //        Description = productDTO.Description,
        //        Price = productDTO.Price,
        //        IsPublish = productDTO.IsPublish,
        //        IsNew = productDTO.IsNew,
        //        Count = productDTO.Count,
        //        CreatedAt = DateTime.UtcNow,
        //        UpdatedAt = DateTime.UtcNow,
        //        ProductImages = new List<ProductImage>(), // Khởi tạo danh sách hình ảnh
        //        ProductCategorys = new List<ProductCategory>() // Khởi tạo danh sách danh mục
        //    };

        //    // Xử lý danh sách hình ảnh - luôn tạo mới Image
        //    if (productDTO.Images != null && productDTO.Images.Any())
        //    {
        //        foreach (var img in productDTO.Images)
        //        {
        //            if (img == null || string.IsNullOrEmpty(img.Link))
        //            {
        //                continue; // Bỏ qua nếu hình ảnh không hợp lệ
        //            }

        //            // Tạo mới Image
        //            var newImage = new Image
        //            {
        //                Link = img.Link,
        //                Name = img.Name ?? "Unnamed", // Gán mặc định nếu Name null
        //                ProductImages = new List<ProductImage>()
        //            };
        //            _context.Images.Add(newImage);
        //            await _context.SaveChangesAsync(); // Lưu để có ImageId

        //            // Tạo ProductImage liên kết với Product và Image mới
        //            var productImage = new ProductImage
        //            {
        //                ProductId = product.Id, // Sẽ được cập nhật sau khi lưu product
        //                ImageId = newImage.Id,
        //                Product = product,
        //                Image = newImage
        //            };
        //            product.ProductImages.Add(productImage);
        //        }
        //    }

        //    // Xử lý danh sách danh mục (ProductCategory)
        //    if (productDTO.ProductCategorys != null && productDTO.ProductCategorys.Any())
        //    {
        //        foreach (var pcDTO in productDTO.ProductCategorys)
        //        {
        //            var existingCategory = await _context.Category
        //                .FirstOrDefaultAsync(c => c.Id == pcDTO.CategoryId);

        //            if (existingCategory != null)
        //            {
        //                var productCategory = new ProductCategory
        //                {
        //                    Product = product,
        //                    Category = existingCategory,
        //                    CategoryId = existingCategory.Id
        //                };
        //                product.ProductCategorys.Add(productCategory);
        //            }
        //        }
        //    }

        //    // Thêm product vào context và lưu để có Id
        //    _context.Products.Add(product);
        //    await _context.SaveChangesAsync();

        //    // Cập nhật ProductId cho các ProductImage sau khi product đã được lưu
        //    foreach (var productImage in product.ProductImages)
        //    {
        //        productImage.ProductId = product.Id;
        //    }
        //    await _context.SaveChangesAsync();

        //    // Cập nhật productDTO để trả về
        //    productDTO.Id = product.Id;
        //    productDTO.ProductCategorys = product.ProductCategorys.Select(pc => new ProductCategoryDTO
        //    {
        //        ProductId = pc.ProductId,
        //        CategoryId = pc.CategoryId,
        //        CategoryName = pc.Category.Name,
        //    }).ToList();

        //    // Thêm thông tin hình ảnh vào DTO
        //    productDTO.Images = product.ProductImages.Select(pi => new ImageDTO
        //    {
        //        Id = pi.Image.Id,
        //        Link = pi.Image.Link,
        //        Name = pi.Image.Name
        //    }).ToList();

        //    return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDTO);
        //}
        // Thêm sản phẩm mới 
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
                ProductCategorys = new List<ProductCategory>()
            };

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

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            foreach (var productImage in product.ProductImages)
            {
                productImage.ProductId = product.Id;
            }
            await _context.SaveChangesAsync();

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

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDTO);
        }

        // PUT: api/products/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateProduct(int id, ProductDTO productDTO)
        //{
        //    if (id != productDTO.Id)
        //    {
        //        return BadRequest();
        //    }

        //    var product = await _context.Products
        //        .Include(p => p.ProductImages)
        //        .Include(p => p.ProductCategorys)
        //        .FirstOrDefaultAsync(p => p.Id == id);

        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    // Cập nhật thông tin sản phẩm
        //    product.PosCode = productDTO.PosCode;
        //    product.Name = productDTO.Name;
        //    product.Description = productDTO.Description;
        //    product.Price = productDTO.Price;
        //    product.IsPublish = productDTO.IsPublish;
        //    product.IsNew = productDTO.IsNew;
        //    product.Count = productDTO.Count;
        //    product.UpdatedAt = DateTime.UtcNow;

        //    // Cập nhật danh sách ảnh
        //    product.ProductImages.Clear();
        //    if (productDTO.Images != null && productDTO.Images.Any())
        //    {
        //        foreach (var image in productDTO.Images)
        //        {
        //            product.ProductImages.Add(new ProductImage
        //            {
        //                ProductId = product.Id,
        //                ImageId = image.Id
        //            });
        //        }
        //    }
        //    // **Cập nhật danh sách danh mục**
        //    // Lấy danh sách CategoryId hiện có
        //    var existingCategoryIds = product.ProductCategorys.Select(pc => pc.CategoryId).ToList();
        //    // Lấy danh sách CategoryId từ DTO (xử lý trường hợp null)
        //    var dtoCategoryIds = productDTO.ProductCategorys?.Select(pc => pc.CategoryId).ToList() ?? new List<int>();
        //    // Xác định các CategoryId cần xóa
        //    var toRemoveCategoryIds = existingCategoryIds.Except(dtoCategoryIds).ToList();
        //    // Xác định các CategoryId cần thêm
        //    var toAddCategoryIds = dtoCategoryIds.Except(existingCategoryIds).ToList();

        //    // Xóa các ProductCategory không còn trong DTO
        //    foreach (var categoryId in toRemoveCategoryIds)
        //    {
        //        var pcToRemove = product.ProductCategorys.FirstOrDefault(pc => pc.CategoryId == categoryId);
        //        if (pcToRemove != null)
        //        {
        //           product.ProductCategorys.Remove(pcToRemove);
        //        }
        //    }
        //    // Thêm mới các ProductCategory từ DTO
        //    foreach (var categoryId in toAddCategoryIds)
        //    {
        //        var newPc = new ProductCategory { ProductId = product.Id, CategoryId = categoryId };
        //        product.ProductCategorys.Add(newPc);
        //    }

        //    _context.Entry(product).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}
        // PUT: api/products/{id}
        [HttpPut("{id}")]
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

    }

}