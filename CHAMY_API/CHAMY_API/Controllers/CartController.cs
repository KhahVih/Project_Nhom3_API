using CHAMY_API.Data;
using CHAMY_API.DTOs;
using CHAMY_API.Models;
using CHAMY_API.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Customer/{id}")]
        public async Task<ActionResult<IEnumerable<CartItemDTO>>> GetCart(int id)
        {
            var cart = await _context.CartItems
             .Where(c => c.CustomerId == id)
             .Include(c => c.Product)
                .ThenInclude(c => c.ProductImages)
             .Include(c => c.Color)
             .Include(c => c.Size)
             .Select(item => new CartItemDTO // Giả sử đây là DTO, tôi đổi tên class cho rõ ràng
             {
                 Id = item.Id,
                 CustomerId = item.CustomerId ?? 0,
                 ProductId = item.ProductId,
                 ProductName = item.Product.Name ?? "Không có tên", // Thêm tên sản phẩm
                 Quantity = item.Quantity,
                 ColorId = item.ColorId,
                 ColorName = item.Color.Name ?? "Không chọn",
                 SizeId = item.SizeId,
                 SizeName = item.Size.Name ?? "Không chọn",
                 UnitPrice = item.UnitPrice,
                 FinalPrice = (item.Product.Sales != null && item.Product.Sales.IsActive &&
                          DateTime.Now >= item.Product.Sales.StartDate && DateTime.Now <= item.Product.Sales.EndDate)
                          ? item.UnitPrice * (1 - item.Product.Sales.DiscountPercentage / 100)
                          : item.UnitPrice,
                 ProductImage = item.Product.ProductImages != null &&  item.Product.ProductImages.Any()
                   ? item.Product.ProductImages.First().Image.Link : ""


             }).ToListAsync();

            return Ok(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(CartItemDTO cartItemDto)
        {
            if (cartItemDto == null || cartItemDto.Quantity <= 0)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ!" });
            }

            var product = await _context.Products
                .Include(p => p.Sales)
                .FirstOrDefaultAsync(p => p.Id == cartItemDto.ProductId);
            if (product == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại!" });
            }

            int? customerId = (cartItemDto.CustomerId == 0 || !cartItemDto.CustomerId.HasValue) ? null : cartItemDto.CustomerId;

            if (customerId.HasValue)
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId.Value);
                if (!customerExists)
                {
                    return BadRequest(new { message = "Khách hàng không tồn tại!" });
                }
            }

            var existingItem = await _context.CartItems
                .Include(c => c.Product)
                .Include(c => c.Color)
                .Include(c => c.Size)
                .FirstOrDefaultAsync(c =>
                    c.CustomerId == cartItemDto.CustomerId &&
                    c.ProductId == cartItemDto.ProductId &&
                    c.ColorId == cartItemDto.ColorId &&
                    c.SizeId == cartItemDto.SizeId);
            CartItemDTO resultItem;
            double UnitPrice = cartItemDto.UnitPrice;
            double FinalPrice = UnitPrice;
            if (product.Sales != null && product.Sales.IsActive && DateTime.Now >= product.Sales.StartDate && DateTime.Now <= product.Sales.EndDate)
            {
                FinalPrice = UnitPrice * (1 - product.Sales.DiscountPercentage / 100);
            }
            if (existingItem != null)
            {
                existingItem.Quantity += cartItemDto.Quantity;
                existingItem.UnitPrice = UnitPrice;
                await _context.SaveChangesAsync();
                resultItem = new CartItemDTO
                {
                    Id = existingItem.Id,
                    CustomerId = existingItem.CustomerId ?? 0,
                    ProductId = existingItem.ProductId,
                    ProductName = existingItem.Product.Name,
                    Quantity = existingItem.Quantity,
                    ColorId = existingItem.ColorId ?? 0,
                    ColorName = existingItem.Color?.Name,
                    SizeId = existingItem.SizeId ?? 0,
                    SizeName = existingItem.Size?.Name,
                    //UnitPrice = existingItem.UnitPrice,
                    //FinalPrice = existingItem.FinalPrice
                    UnitPrice = UnitPrice,
                    FinalPrice = FinalPrice, // Sử dụng giá cuối cùng đã tính 
                };
            }
            else
            {
                var newCartItem = new CartItem
                {
                    CustomerId = customerId,
                    ProductId = cartItemDto.ProductId,
                    Quantity = cartItemDto.Quantity,
                    ColorId = cartItemDto.ColorId,
                    SizeId = cartItemDto.SizeId,
                    // UnitPrice = cartItemDto.UnitPrice // Gán giá từ product nếu có sale
                    UnitPrice = UnitPrice,
                    FinalPrice= FinalPrice,
                };
                _context.CartItems.Add(newCartItem);
                await _context.SaveChangesAsync();
                resultItem = new CartItemDTO
                {
                    Id = newCartItem.Id,
                    CustomerId = newCartItem.CustomerId ?? 0,
                    ProductId = newCartItem.ProductId,
                    ProductName = product.Name,
                    Quantity = newCartItem.Quantity,
                    ColorId = newCartItem.ColorId,
                    ColorName = newCartItem.ColorId.HasValue ? (await _context.Colors.FindAsync(newCartItem.ColorId))?.Name : null,
                    SizeId = newCartItem.SizeId,
                    SizeName = newCartItem.SizeId.HasValue ? (await _context.Sizes.FindAsync(newCartItem.SizeId))?.Name : null,
                    //UnitPrice = newCartItem.UnitPrice,
                    //FinalPrice = newCartItem.UnitPrice // Có thể tính lại nếu cần
                    UnitPrice = UnitPrice,
                    FinalPrice = FinalPrice // áp dụng giá đã giảm 
                };
            }


            return Ok(resultItem);
        }

        //[HttpPost]
        //public async Task<IActionResult> AddToCart(CartItemDTO cartItemDto)
        //{
        //    // Log input for debugging
        //    Console.WriteLine($"cartItemDto: {JsonSerializer.Serialize(cartItemDto)}");

        //    // Validate input
        //    if (cartItemDto == null || cartItemDto.Quantity <= 0)
        //    {
        //        return BadRequest(new { message = "Dữ liệu không hợp lệ!" });
        //    }

        //    // Validate UnitPrice
        //    if (cartItemDto.UnitPrice <= 0)
        //    {
        //        return BadRequest(new { message = "Giá sản phẩm không hợp lệ!" });
        //    }

        //    // Validate ProductId and fetch product details
        //    var product = await _context.Products
        //        .Include(p => p.Sales)
        //        .Include(p => p.ProductImages)
        //        .FirstOrDefaultAsync(p => p.Id == cartItemDto.ProductId);
        //    if (product == null)
        //    {
        //        return NotFound(new { message = "Sản phẩm không tồn tại!" });
        //    }

        //    // Handle CustomerId: Treat 0 or null as guest user (store as NULL)
        //    int? customerId = (cartItemDto.CustomerId == 0 || !cartItemDto.CustomerId.HasValue)
        //        ? null
        //        : cartItemDto.CustomerId;

        //    // Validate CustomerId if provided (non-zero, non-null)
        //    if (customerId.HasValue)
        //    {
        //        var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId.Value);
        //        if (!customerExists)
        //        {
        //            return BadRequest(new { message = "Khách hàng không tồn tại!" });
        //        }
        //    }

        //    // Check for existing cart item
        //    var existingItem = await _context.CartItems
        //        .Include(c => c.Product)
        //        .Include(c => c.Color)
        //        .Include(c => c.Size)
        //        .FirstOrDefaultAsync(c =>
        //            c.CustomerId == customerId &&
        //            c.ProductId == cartItemDto.ProductId &&
        //            c.ColorId == cartItemDto.ColorId &&
        //            c.SizeId == cartItemDto.SizeId);

        //    CartItemDTO resultItem;
        //    double unitPrice = cartItemDto.UnitPrice; // Or use product.Price for consistency
        //    double finalPrice = unitPrice;

        //    // Calculate FinalPrice if there’s an active sale
        //    if (product.Sales != null && product.Sales.IsActive &&
        //        DateTime.Now >= product.Sales.StartDate && DateTime.Now <= product.Sales.EndDate)
        //    {
        //        if (product.Sales.DiscountPercentage.HasValue)
        //        {
        //            finalPrice = unitPrice * (1 - product.Sales.DiscountPercentage.Value / 100);
        //        }
        //        else
        //        {
        //            return BadRequest(new { message = "Khuyến mãi không hợp lệ!" });
        //        }
        //    }

        //    if (existingItem != null)
        //    {
        //        // Update existing item
        //        existingItem.Quantity += cartItemDto.Quantity;
        //        existingItem.UnitPrice = unitPrice;
        //        //existingItem.FinalPrice = finalPrice;
        //        await _context.SaveChangesAsync();

        //        resultItem = new CartItemDTO
        //        {
        //            Id = existingItem.Id,
        //            CustomerId = existingItem.CustomerId,
        //            ProductId = existingItem.ProductId,
        //            ProductName = existingItem.Product.Name,
        //            Quantity = existingItem.Quantity,
        //            ColorId = existingItem.ColorId,
        //            ColorName = existingItem.Color?.Name ?? "Không chọn",
        //            SizeId = existingItem.SizeId,
        //            SizeName = existingItem.Size?.Name ?? "Không chọn",
        //            UnitPrice = existingItem.UnitPrice,
        //            FinalPrice = existingItem.FinalPrice,
        //        };
        //    }
        //    else
        //    {
        //        // Create new cart item
        //        var newCartItem = new CartItem
        //        {
        //            CustomerId = customerId, // Null for guest users (0 or null in DTO)
        //            ProductId = cartItemDto.ProductId,
        //            Quantity = cartItemDto.Quantity,
        //            ColorId = cartItemDto.ColorId,
        //            SizeId = cartItemDto.SizeId,
        //            UnitPrice = unitPrice,
        //            FinalPrice = finalPrice
        //        };

        //        _context.CartItems.Add(newCartItem);
        //        await _context.SaveChangesAsync();

        //        // Fetch ColorName and SizeName
        //        var colorName = newCartItem.ColorId.HasValue
        //            ? (await _context.Colors.FindAsync(newCartItem.ColorId))?.Name ?? "Không chọn"
        //            : "Không chọn";
        //        var sizeName = newCartItem.SizeId.HasValue
        //            ? (await _context.Sizes.FindAsync(newCartItem.SizeId))?.Name ?? "Không chọn"
        //            : "Không chọn";

        //        resultItem = new CartItemDTO
        //        {
        //            Id = newCartItem.Id,
        //            CustomerId = newCartItem.CustomerId,
        //            ProductId = newCartItem.ProductId,
        //            ProductName = product.Name,
        //            Quantity = newCartItem.Quantity,
        //            ColorId = newCartItem.ColorId,
        //            ColorName = colorName,
        //            SizeId = newCartItem.SizeId,
        //            SizeName = sizeName,
        //            UnitPrice = newCartItem.UnitPrice,
        //            FinalPrice = newCartItem.FinalPrice,
                    
        //        };
        //    }

        //    return Ok(resultItem);
        //}

         // Cập nhật số lượng sản phẩm trong giỏ hàng
        [HttpPut("{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ hàng!" });
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Giỏ hàng đã được cập nhật!" });
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ hàng!" });
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sản phẩm đã được xóa khỏi giỏ hàng!" });
        }
    }
}