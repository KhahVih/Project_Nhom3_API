using CHAMY_API.Data;
using CHAMY_API.Models;
using CHAMY_API.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;


        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut(CheckOutDTO checkoutDto)
        {
            if (checkoutDto == null || !checkoutDto.CartItems.Any())
            {
                return BadRequest(new { message = "Giỏ hàng trống hoặc dữ liệu không hợp lệ!" });
            }
            Console.WriteLine($"CustomerId: {checkoutDto.CustomerId}"); // Debug
            foreach (var item in checkoutDto.CartItems)
            {
                Console.WriteLine($"ProductId: {item.ProductId}, UnitPrice: {item.UnitPrice}, Quantity: {item.Quantity}");
            }
            // Tạo Order mới
            // Không cần kiểm tra CustomerId tồn tại nếu là khách chưa đăng nhập 
            var order = new Order
            {
                //CustomerId = checkoutDto.CustomerId, // Có thể null nếu chưa đăng nhập
                //nếu như checkoutDto.CustomerId == null thì gán giá trị cho  CustomerId = null, ngược lại thì gán CustomerId = checkoutDto.CustomerId,
                CustomerId = checkoutDto.CustomerId.HasValue && _context.Customers.Any(c => c.Id == checkoutDto.CustomerId.Value)
                    ? checkoutDto.CustomerId
                    : null, // Chỉ gán nếu tồn tại, nếu không thì null
                CustomerName = checkoutDto.CustomerName,
                Email = checkoutDto.Email,
                Province = checkoutDto.Province,
                District = checkoutDto.District,
                Wards = checkoutDto.Wards,
                Address = checkoutDto.Address,
                Phone = checkoutDto.Phone,
                Note = checkoutDto.Note,
                TotalAmount = checkoutDto.CartItems.Sum(item => item.UnitPrice * item.Quantity),
                Status = OrderStatus.Pending, // Mặc định là Pending
                CreatedAt = DateTime.UtcNow,
            };


            // Lấy danh sách OrderItems và tham chiếu tên từ Product, Color, Size
            var orderItems = new List<OrderDetail>();
            foreach (var item in checkoutDto.CartItems)
            {
                // Lấy thông tin Product
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product == null)
                {
                    return BadRequest(new { message = $"Sản phẩm với ID {item.ProductId} không tồn tại!" });
                }

                // Lấy thông tin Color (nếu có)
                //string colorName = null;
                //if (item.ColorId.HasValue)
                //{
                //    var color = await _context.Colors
                //        .FirstOrDefaultAsync(c => c.Id == item.ColorId.Value);
                //    colorName = color?.Name;
                //}

                //// Lấy thông tin Size (nếu có)
                //string sizeName = null;
                //if (item.SizeId.HasValue)
                //{
                //    var size = await _context.Sizes
                //        .FirstOrDefaultAsync(s => s.Id == item.SizeId.Value);
                //    sizeName = size?.Name;
                //}

                // Tạo OrderDetail
                var orderDetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    ColorId = item.ColorId,
                    SizeId = item.SizeId,
                    // Không cần lưu tên trực tiếp vào OrderDetail vì thông tin này có thể lấy từ bảng liên quan
                };
                orderItems.Add(orderDetail);
            }

            order.OrderItems = orderItems;
            _context.Orders.Add(order);

            // Xóa CartItem nếu đã đăng nhập
            if (checkoutDto.CustomerId.HasValue && checkoutDto.CustomerId != 0)
            {
                var cartItems = await _context.CartItems
                    .Where(c => c.CustomerId == checkoutDto.CustomerId)
                    .ToListAsync();
                _context.CartItems.RemoveRange(cartItems);
            }

            //await _context.SaveChangesAsync();
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); // Debug lỗi chi tiết
                throw;
            }

            // Trả về OrderDetailDTO với thông tin tên
            var orderDetailDtos = order.OrderItems.Select(od => new OrderDetailDTO
            {
                Id = od.Id,
                ProductId = od.ProductId,
                ProductName = _context.Products.FirstOrDefault(p => p.Id == od.ProductId)?.Name,
                ProductPosCode = _context.Products.FirstOrDefault(p => p.Id == od.ProductId)?.PosCode,
                Quantity = od.Quantity,
                UnitPrice = od.UnitPrice,
                ColorId = od.ColorId,
                ColorName = od.ColorId.HasValue ? _context.Colors.FirstOrDefault(c => c.Id == od.ColorId.Value)?.Name : null,
                SizeId = od.SizeId,
                SizeName = od.SizeId.HasValue ? _context.Sizes.FirstOrDefault(s => s.Id == od.SizeId.Value)?.Name : null
            }).ToList();

            return Ok(new
            {
                message = "Đơn hàng đã được tạo thành công!",
                orderId = order.Id,
                orderItems = orderDetailDtos
            });
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int page = 1)
        {
            const int pageSize = 6;
            if (page < 1) page = 1;
            var query = _context.Orders
                .OrderByDescending(o => o.Id)
                .Include(o => o.OrderItems)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(od => od.Color)
                .Include(o => o.OrderItems)
                    .ThenInclude(od => od.Size);
            // tổng đơn hàng 
            var totalOrder = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalOrder / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            var orders = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerId,
                    o.CustomerName,
                    o.Email,
                    o.Province,
                    o.District,
                    o.Wards,
                    o.Address,
                    o.Phone,
                    o.Note,
                    o.TotalAmount,
                    Status = o.Status.GetDisplayName(),
                    o.CreatedAt,
                    OrderItems = o.OrderItems.Select(od => new OrderDetailDTO
                    {
                        Id = od.Id,
                        ProductId = od.ProductId,
                        ProductName = od.Product.Name,
                        ProductPosCode = od.Product.PosCode,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        ColorId = od.ColorId,
                        ColorName = od.Color.Name,
                        SizeId = od.SizeId,
                        SizeName = od.Size.Name,
                    }).ToList()
                })
                .ToListAsync();
            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalOrders = totalOrder, // Tổng số đơn hàng  
                Orders = orders // Danh sách đơn hàng  
            };

            return Ok(result);
        }
        [HttpGet("getorderbyid/{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var orders = await _context.Orders
                .Where(o => o.Id == id)
                .Include(o => o.OrderItems)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(od => od.Color)
                .Include(o => o.OrderItems)
                    .ThenInclude(od => od.Size)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerId,
                    o.CustomerName,
                    o.Email,
                    o.Province,
                    o.District,
                    o.Wards,
                    o.Address,
                    o.Phone,
                    o.Note,
                    o.TotalAmount,
                    Status = o.Status.GetDisplayName(),
                    o.CreatedAt,
                    OrderItems = o.OrderItems.Select(od => new OrderDetailDTO
                    {
                        Id = od.Id,
                        ProductId = od.ProductId,
                        ProductName = od.Product.Name,
                        ProductPosCode = od.Product.PosCode,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        ColorId = od.ColorId,
                        ColorName = od.Color.Name,
                        SizeId = od.SizeId,
                        SizeName = od.Size.Name,
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("getorderbycustomer/{id}")]
        public async Task<IActionResult> GetOrderByCustomer(int id)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerId == id)
                .Include(o => o.OrderItems)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(od => od.Color)
                .Include(o => o.OrderItems)
                    .ThenInclude(od => od.Size)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerId,
                    o.CustomerName,
                    o.Email,
                    o.Province,
                    o.District,
                    o.Wards,
                    o.Address,
                    o.Phone,
                    o.Note,
                    o.TotalAmount,
                    Status = o.Status.GetDisplayName(),
                    o.CreatedAt,
                    OrderItems = o.OrderItems.Select(od => new OrderDetailDTO
                    {
                        Id = od.Id,
                        ProductId = od.ProductId,
                        ProductName = od.Product.Name,
                        ProductPosCode = od.Product.PosCode,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        ColorId = od.ColorId,
                        ColorName = od.Color.Name,
                        SizeId = od.SizeId,
                        SizeName = od.Size.Name,
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }


        [HttpPut("update-status/{orderId}")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateOrderStatusDTO updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest(new { message = "Dữ liệu cập nhật không hợp lệ!" });
            }

            var order = await _context.Orders
             .Include(o => o.OrderItems)
             .ThenInclude(oi => oi.Product)
             .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {orderId}!" });
            }

            order.Status = updateDto.Status;
            // Nếu trạng thái mới là "Paid", cập nhật số lượng sản phẩm
            if (updateDto.Status == OrderStatus.Processing)
            {
                foreach (var orderItem in order.OrderItems)
                {
                    var product = orderItem.Product;
                    if (product == null)
                    {
                        return BadRequest(new { message = $"Sản phẩm với ID {orderItem.ProductId} không tồn tại!" });
                    }

                    if (product.Count < orderItem.Quantity)
                    {
                        return BadRequest(new
                        {
                            message = $"Sản phẩm {product.Name} không đủ số lượng tồn kho! Hiện có: {product.Count}, Yêu cầu: {orderItem.Quantity}"
                        });
                    }

                    product.Count -= orderItem.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(product).State = EntityState.Modified;
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật trạng thái!", error = ex.Message });
            }

            return Ok(new
            {
                message = "Cập nhật trạng thái đơn hàng thành công!",
                orderId = order.Id,
                newStatus = order.Status
            });
        }
        // GET: api/order/count-by-status
        [HttpGet("count-by-status")]
        public async Task<IActionResult> GetOrderCountByStatus()
        {
            try
            {
                var pendingCount = await _context.Orders
                    .CountAsync(o => o.Status == OrderStatus.Pending);

                var deliveredCount = await _context.Orders
                    .CountAsync(o => o.Status == OrderStatus.Delivered);

                return Ok(new
                {
                    pendingOrders = pendingCount,
                    deliveredOrders = deliveredCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi đếm đơn hàng: {ex.Message}");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi đếm đơn hàng", error = ex.Message });
            }
        }

        [HttpGet("by-statusPending")]
        public async Task<IActionResult> GetOrdersByStatusPending()
        {
            var pendingOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Color)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Size)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerId,
                    o.CustomerName,
                    o.Email,
                    o.Province,
                    o.District,
                    o.Wards,
                    o.Address,
                    o.Phone,
                    o.Note,
                    o.TotalAmount,
                    Status = o.Status.GetDisplayName(),
                    o.CreatedAt,
                    OrderItems = o.OrderItems.Select(od => new OrderDetailDTO
                    {
                        Id = od.Id,
                        ProductId = od.ProductId,
                        ProductName = od.Product.Name,
                        ProductPosCode = od.Product.PosCode,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        ColorId = od.ColorId,
                        ColorName = od.Color.Name,
                        SizeId = od.SizeId,
                        SizeName = od.Size.Name,
                    }).ToList()
                })
                .ToListAsync();

            //var approvedOrders = await _context.Orders
            //    .Where(o => o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered)
            //    .Include(o => o.OrderItems)
            //        .ThenInclude(oi => oi.Product)
            //    .Include(o => o.OrderItems)
            //        .ThenInclude(oi => oi.Color)
            //    .Include(o => o.OrderItems)
            //        .ThenInclude(oi => oi.Size)
            //    .Select(o => new
            //    {
            //        o.Id,
            //        o.CustomerId,
            //        o.CustomerName,
            //        o.Address,
            //        o.Phone,
            //        o.TotalAmount,
            //        o.Status,
            //        o.CreatedAt,
            //        OrderItems = o.OrderItems.Select(od => new OrderDetailDTO
            //        {
            //            Id = od.Id,
            //            ProductId = od.ProductId,
            //            ProductName = od.Product.Name,
            //            Quantity = od.Quantity,
            //            UnitPrice = od.UnitPrice,
            //            ColorId = od.ColorId,
            //            ColorName = od.Color.Name,
            //            SizeId = od.SizeId,
            //            SizeName = od.Size.Name,
            //        }).ToList()
            //    })
            //    .ToListAsync();

            return Ok(pendingOrders);
        }

        [HttpGet("by-statusDelivered{page}")]
        public async Task<IActionResult> GetOrdersByStatusDelivered(int page = 1)
        {
            const int pageSize = 6;
            if (page < 1) page = 1;
            var query = _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered) //o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped ||
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Color)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Size);
            // tổng đơn hàng 
            var totalOrder = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalOrder / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            var processingOrders = await query
                .Skip(skip)
                .Take(totalPage)
                .Select(o => new
                {
                    o.Id,
                    o.CustomerId,
                    o.CustomerName,
                    o.Email,
                    o.Province,
                    o.District,
                    o.Wards,
                    o.Address,
                    o.Phone,
                    o.Note,
                    o.TotalAmount,
                    Status = o.Status.GetDisplayName(),
                    o.CreatedAt,
                    OrderItems = o.OrderItems.Select(od => new OrderDetailDTO
                    {
                        Id = od.Id,
                        ProductId = od.ProductId,
                        ProductName = od.Product.Name,
                        ProductPosCode = od.Product.PosCode,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        ColorId = od.ColorId,
                        ColorName = od.Color.Name,
                        SizeId = od.SizeId,
                        SizeName = od.Size.Name,
                    }).ToList()
                })
                .ToListAsync();
            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalOrders = totalOrder, // Tổng số đơn hàng  
                ProcessingOrders = processingOrders // Danh sách đơn hàng đã được giao  
            };
            return Ok(result);
        }

        // DELETE: api/order/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                // Tìm order theo id, bao gồm cả OrderItems
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound(new { message = $"Không tìm thấy đơn hàng với ID {id}" });
                }

                // Xóa các OrderItems liên quan trước (nếu database không tự động cascade delete)
                if (order.OrderItems != null && order.OrderItems.Any())
                {
                    _context.OrderDetails.RemoveRange(order.OrderItems);
                }

                // Xóa order
                _context.Orders.Remove(order);

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Đơn hàng {id} đã được xóa thành công",
                    deletedOrderId = id
                });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                Console.WriteLine($"Lỗi khi xóa đơn hàng {id}: {ex.Message}");

                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi xóa đơn hàng",
                    error = ex.Message
                });
            }
        }
    }
}