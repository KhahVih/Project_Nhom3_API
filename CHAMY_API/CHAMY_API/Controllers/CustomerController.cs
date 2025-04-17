using CHAMY_API.Data;
using CHAMY_API.Models.DTO;
using CHAMY_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/customers - Lấy danh sách tất cả customer cùng với comments của họ
        [HttpGet("GetCustomer/page{page}")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomers(int page = 1)
        {
            const int pageSize = 8;
            if (page < 1) page = 1;
            var query = _context.Customers
                .Include(c => c.Comments);
            // tổng số khách hàng 
            var totalCustomer = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalCustomer / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            var customers = await  query
                .Skip(skip)
                .Take(pageSize)
                .Select(c => new CustomerDTO
                {
                    Id = c.Id,
                    Username = c.Username,
                    Email = c.Email,
                    Fullname = c.Fullname,
                    Password = c.Password,
                    Address = c.Address,
                    Phone = c.Phone,
                    Gender = c.Gender,
                    IsClone = c.IsClone,
                    CreatedAt = c.CreatedAt,
                    Date = c.Date,
                    CommentCount = c.Comments.Count(com => com.CustomerId == c.Id), // Chỉ đếm comment hợp lệ
                    Comments = c.Comments
                        .Where(com => com.CustomerId == c.Id) // Lọc comment khớp với CustomerId
                        .Select(com => new CommentDTO
                        {
                            Id = com.Id,
                            ProductId = com.ProductId,
                            Vote = com.Vote,
                            Description = com.Description,
                            CustomerId = com.CustomerId,
                            CreatedAt = com.CreatedAt,
                            IsShow = com.IsShow
                        }).ToList(),
                })
                .ToListAsync();

            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang 
                ToTalCustomers = totalCustomer, // Tổng số khách hàng 
                Customers = customers, // danh sách khách hàng 
            };

            return Ok(result);
        }
        // GET: api/customers/5 - Lấy thông tin chi tiết một customer
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDTO>> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Comments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            var customerDTO = new CustomerDTO
            {

                Id = customer.Id,
                Username = customer.Username,
                Email = customer.Email,
                Fullname = customer.Fullname,
                Password = customer.Password,
                Address = customer.Address,
                Phone = customer.Phone,
                Gender = customer.Gender,
                IsClone = customer.IsClone,
                CreatedAt = customer.CreatedAt,
                Date = customer.Date,
                CommentCount = customer.Comments.Count(com => com.CustomerId == customer.Id), // Chỉ đếm comment hợp lệ
                Comments = customer.Comments
                        .Where(com => com.CustomerId == customer.Id) // Lọc comment khớp với CustomerId
                        .Select(com => new CommentDTO
                        {
                            Id = com.Id,
                            ProductId = com.ProductId,
                            Vote = com.Vote,
                            Description = com.Description,
                            CustomerId = com.CustomerId,
                            CreatedAt = com.CreatedAt,
                            IsShow = com.IsShow
                        }).ToList()
            };

            return Ok(customerDTO);
        }

        // POST: api/customers - Tạo mới customer
        [HttpPost]
        public async Task<ActionResult<CustomerDTO>> CreateCustomer(CustomerDTO customerDTO)
        {
            var customer = new Customer
            {
                Username = customerDTO.Username,
                Email = customerDTO.Email,
                Password =  customerDTO.Password, // Lưu ý: Nên mã hóa password trong thực tế
                Fullname = customerDTO.Fullname,
                Address = customerDTO.Address,
                Phone = customerDTO.Phone,
                Gender = customerDTO.Gender,
                IsClone = false,
                CreatedAt = DateTime.UtcNow,
                Date = customerDTO.Date,
                //Comments = new List<Comment>()
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            customerDTO.Id = customer.Id;
            customerDTO.CreatedAt = customer.CreatedAt;
            customerDTO.CommentCount = 0;
            customerDTO.Comments = null;

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customerDTO);
        }

        // PUT: api/customers/5 - Cập nhật thông tin customer
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, CustomerDTO customerDTO)
        {
            if (id != customerDTO.Id)
            {
                return BadRequest("ID trong URL không khớp với ID trong body");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // Cập nhật các thuộc tính
            customer.Username = customerDTO.Username;
            customer.Email = customerDTO.Email;
            customer.Fullname = customerDTO.Fullname;
            customer.Password = customerDTO.Password;
            customer.Address = customerDTO.Address;
            customer.Phone = customerDTO.Phone;
            customer.Gender = customerDTO.Gender;
            customer.IsClone = customerDTO.IsClone;
            customer.Date = customerDTO.Date;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { message = "Email, mật khẩu cũ và mật khẩu mới là bắt buộc" });
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);
            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng với email này" });
            }

            // Kiểm tra mật khẩu cũ
            if (request.OldPassword != customer.Password)
            {
                return BadRequest(new { message = "Mật khẩu cũ không đúng" });
            }

            customer.Password = request.NewPassword;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(customer.Id))
                {
                    return NotFound(new { message = "Không tìm thấy khách hàng" });
                }
                throw;
            }

            return Ok(new { message = "Mật khẩu đã được cập nhật thành công" });
        }

        // DELETE: api/customers/5 - Xóa customer
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper method để kiểm tra customer tồn tại
        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
