using CHAMY_API.Data;
using CHAMY_API.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest(new { message = "Tên đăng nhập và mật khẩu là bắt buộc!" });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == loginDto.Username && c.Password == loginDto.Password);

            if (customer == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng!" });
            }

            var customerDto = new CustomerDTO
            {
                Id = customer.Id,
                Username = customer.Username,
                Email = customer.Email,
                Fullname = customer.Fullname,
                Address = customer.Address,
                Phone = customer.Phone,
                Gender = customer.Gender,
                IsClone = customer.IsClone,
                CreatedAt = customer.CreatedAt,
                Date = customer.Date
            };

            return Ok(new { message = "Đăng nhập thành công!", customer = customerDto });
        }

        [HttpPost("admin/login")]
        public async Task<IActionResult> LoginAdmin(LoginDTO login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Tìm user trong database
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.UserName == login.Username && u.Password == login.Password);
            if (user == null)
            {
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng");
            }
            var userpermission = user.UserPermissions.FirstOrDefault();
            string role = "";
            if (userpermission != null) 
            {
                switch (userpermission.PermissionId)
                {
                    case 1:
                        role = "admin";
                        break;
                    case 2:
                        role = "Employee";
                        break;
                }
            }

            var response = new
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Role = role,
                IsAdmin = userpermission?.PermissionId == 1,
            };

            return Ok(response);

        }

    }
}
