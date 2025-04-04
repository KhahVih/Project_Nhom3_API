using CHAMY_API.Data;
using CHAMY_API.Models;
using CHAMY_API.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🟢 Lấy danh sách Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Password = u.Password,
                    Email = u.Email,
                    IsAdmin = u.IsAdmin,
                    UserPermissions = u.UserPermissions.Select(up => new UserPermissionDTO
                    {
                        UserId = up.UserId,
                        PermissionId = up.PermissionId,
                        PermissionName = up.Permission.Name
                    }).ToList()
                }).ToListAsync();

            return Ok(users);
        }

        // 🟢 Lấy User theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User không tồn tại.");
            }

            var userDTO = new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Password = user.Password,
                Email = user.Email,
                IsAdmin = user.IsAdmin,
                UserPermissions = user.UserPermissions.Select(up => new UserPermissionDTO
                {
                    UserId = up.UserId,
                    PermissionId = up.PermissionId,
                    PermissionName = up.Permission.Name
                }).ToList()
            };

            return Ok(userDTO);
        }
        
        // 🟢 Tạo User mới
        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser(UserDTO userDto)
        {

            var newUser = new User
            {
                UserName = userDto.UserName,
                FullName = userDto.FullName,
                Email = userDto.Email,
                Password = userDto.Password, // Mã hóa trước khi lưu
                IsAdmin = false,
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(); // Lưu để lấy ID của User

            // Thêm quyền cho user nếu có
            if (userDto.UserPermissions.Any())
            {
                var userPermissions = userDto.UserPermissions.Select(up => new UserPermission
                {
                    UserId = newUser.Id,
                    PermissionId = up.PermissionId
                }).ToList();

                _context.UserPermissions.AddRange(userPermissions);
                await _context.SaveChangesAsync(); // Lưu quyền vào DB
            }

            // 🟢 Lấy lại User kèm danh sách quyền để trả về DTO
            var createdUser = await _context.Users
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission) // Lấy thông tin Permission
                .FirstOrDefaultAsync(u => u.Id == newUser.Id);

            var createdUserDto = new UserDTO
            {
                Id = createdUser.Id,
                UserName = createdUser.UserName,
                FullName = createdUser.FullName,
                Password = createdUser.Password,
                Email = createdUser.Email,
                IsAdmin = createdUser.IsAdmin,
                UserPermissions = createdUser.UserPermissions.Select(up => new UserPermissionDTO
                {
                    UserId = up.UserId,
                    PermissionId = up.PermissionId,
                    PermissionName = up.Permission.Name // Lấy tên quyền
                }).ToList()
            };

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUserDto);
        }

        // 🟢 Cập nhật User và quyền hạn
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDTO userDto)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User không tồn tại.");
            }

            // Cập nhật thông tin cơ bản
            if (userDto.FullName != null) user.FullName = userDto.FullName;
            if (userDto.Email != null) user.Email = userDto.Email;

            // Xóa quyền cũ
            _context.UserPermissions.RemoveRange(user.UserPermissions);

            // Thêm quyền mới nếu có
            if (userDto.UserPermissions.Any())
            {
                var newPermissions = userDto.UserPermissions.Select(pid => new UserPermission
                {
                    UserId = user.Id,
                    PermissionId = pid.PermissionId,

                }).ToList();

                _context.UserPermissions.AddRange(newPermissions);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("update-password/{id}")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return BadRequest("Mật khẩu không được để trống.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User không tồn tại.");
            }

            // Cập nhật mật khẩu (nên mã hóa nếu dùng thực tế)
            user.Password = newPassword;

            await _context.SaveChangesAsync();
            return Ok("Mật khẩu đã được cập nhật thành công.");
        }

        // 🟢 Xóa User
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User không tồn tại.");
            }

            // Xóa quyền của user trước
            _context.UserPermissions.RemoveRange(user.UserPermissions);

            // Xóa user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
