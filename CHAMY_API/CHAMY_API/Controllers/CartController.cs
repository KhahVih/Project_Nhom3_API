using CHAMY_API.Data;
using CHAMY_API.DTOs;
using CHAMY_API.Models;
using CHAMY_API.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [HttpGet("GetColor")]
        public async Task<ActionResult<IEnumerable<ColorDTO>>> GetColor()
        {
            var color = await _context.Colors
                .ToListAsync();


            return Ok(color);
        }
        [HttpGet("GetSize")]
        public async Task<ActionResult<IEnumerable<ColorDTO>>> GetSize()
        {
            var size = await _context.Sizes
                .ToListAsync();


            return Ok(size);
        }

        [HttpGet("Customer/{id}")]
        public async Task<ActionResult<IEnumerable<CartItemDTO>>> GetCart(int id)
        {
            var cart = await _context.CartItems
             .Where(c => c.CustomerId == id)
             .Include(c => c.Product)
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
                 FinalPrice = item.FinalPrice
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
            if (existingItem != null)
            {
                existingItem.Quantity += cartItemDto.Quantity;
                await _context.SaveChangesAsync();
                resultItem = new CartItemDTO
                {
                    Id = existingItem.Id,
                    CustomerId = existingItem.CustomerId ?? 0,
                    ProductId = existingItem.ProductId,
                    ProductName = existingItem.Product.Name,
                    Quantity = existingItem.Quantity,
                    ColorId = existingItem.ColorId,
                    ColorName = existingItem.Color?.Name,
                    SizeId = existingItem.SizeId,
                    SizeName = existingItem.Size?.Name,
                    UnitPrice = existingItem.UnitPrice,
                    FinalPrice = existingItem.FinalPrice
                };
            }
            else
            {
                var newCartItem = new CartItem
                {
                    CustomerId = cartItemDto.CustomerId,
                    ProductId = cartItemDto.ProductId,
                    Quantity = cartItemDto.Quantity,
                    ColorId = cartItemDto.ColorId,
                    SizeId = cartItemDto.SizeId,
                    UnitPrice = cartItemDto.UnitPrice // Gán giá từ product nếu có sale
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
                    UnitPrice = newCartItem.UnitPrice,
                    FinalPrice = newCartItem.UnitPrice // Có thể tính lại nếu cần
                };
            }

            
            return Ok(resultItem);
        }

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