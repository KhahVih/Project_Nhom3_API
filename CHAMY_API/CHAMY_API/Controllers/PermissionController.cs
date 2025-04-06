using CHAMY_API.Models.DTO;
using CHAMY_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CHAMY_API.Data;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PermissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Permission
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PermissionDTO>>> GetPermissions()
        {
            var permissions = await _context.Permissions
                .Include(p => p.PermissionRoles)
                .ThenInclude(pr => pr.Role) // Đảm bảo lấy Role.Name
                .Select(p => new PermissionDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    PermissionRoles = p.PermissionRoles.Select(pr => new PermissionRoleDTO
                    {
                        PermissionId = pr.PermissionId,
                        RoleId = pr.RoleId,
                        RoleName = pr.Role.Name,
                    }).ToList()
                })
                .ToListAsync();
            return Ok(permissions);
        }

        // GET: api/Permission/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionDTO>> GetPermission(int id)
        {
            var permission = await _context.Permissions
                .Include(p => p.PermissionRoles)
                    .ThenInclude(pr => pr.Role) // Đảm bảo lấy Role.Name
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null)
            {
                return NotFound();
            }

            var permissionDto = new PermissionDTO
            {
                Id = permission.Id,
                Name = permission.Name,
                PermissionRoles = permission.PermissionRoles.Select(pr => new PermissionRoleDTO
                {
                    PermissionId = pr.PermissionId,
                    RoleId = pr.RoleId,
                    RoleName = pr.Role.Name,
                }).ToList()
            };

            return Ok(permissionDto);
        }

        // POST: api/Permission
        [HttpPost]
        public async Task<ActionResult<PermissionDTO>> CreatePermission(PermissionDTO permissionDto)
        {
            // Tạo mới Permission
            var permission = new Permission
            {
                Name = permissionDto.Name
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync(); // Lưu để lấy Id của Permission

            // Nếu có danh sách RoleId, thêm vào PermissionRoles
            if (permissionDto.PermissionRoles != null && permissionDto.PermissionRoles.Any())
            {
                var permissionRoles = new List<PermissionRole>();

                foreach (var roleDto in permissionDto.PermissionRoles)
                {
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleDto.RoleId);
                    if (role == null)
                    {
                        return BadRequest($"Role với Id {roleDto.RoleId} không tồn tại.");
                    }

                    permissionRoles.Add(new PermissionRole
                    {
                        PermissionId = permission.Id,
                        RoleId = roleDto.RoleId
                    });
                }

                _context.PermissionRoles.AddRange(permissionRoles);
                await _context.SaveChangesAsync(); // Lưu toàn bộ danh sách PermissionRole
            }

            // Lấy lại Permission kèm PermissionRoles để trả về DTO
            var createdPermission = await _context.Permissions
                .Include(p => p.PermissionRoles)
                .ThenInclude(pr => pr.Role)
                .FirstOrDefaultAsync(p => p.Id == permission.Id);

            var createdPermissionDto = new PermissionDTO
            {
                Id = createdPermission.Id,
                Name = createdPermission.Name,
                PermissionRoles = createdPermission.PermissionRoles.Select(pr => new PermissionRoleDTO
                {
                    PermissionId = pr.PermissionId,
                    RoleId = pr.RoleId,
                    RoleName = pr.Role.Name,
                }).ToList()
            };

            return CreatedAtAction(nameof(GetPermission), new { id = createdPermission.Id }, createdPermissionDto);
        }

        // PUT: api/Permission/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(int id, PermissionDTO permissionDto)
        {
            if (id != permissionDto.Id)
            {
                return BadRequest("ID không khớp.");
            }

            var permission = await _context.Permissions
                .Include(p => p.PermissionRoles) // Lấy luôn PermissionRoles
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null)
            {
                return NotFound();
            }

            // Cập nhật tên Permission
            permission.Name = permissionDto.Name;

            // Xóa danh sách PermissionRoles cũ
            _context.PermissionRoles.RemoveRange(permission.PermissionRoles);

            // Thêm danh sách PermissionRoles mới
            if (permissionDto.PermissionRoles != null && permissionDto.PermissionRoles.Any())
            {
                var newPermissionRoles = new List<PermissionRole>();

                foreach (var roleDto in permissionDto.PermissionRoles)
                {
                    var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleDto.RoleId);
                    if (!roleExists)
                    {
                        return BadRequest($"Role với Id {roleDto.RoleId} không tồn tại.");
                    }

                    newPermissionRoles.Add(new PermissionRole
                    {
                        PermissionId = permission.Id,
                        RoleId = roleDto.RoleId
                    });
                }

                _context.PermissionRoles.AddRange(newPermissionRoles);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Permission/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _context.Permissions
                .Include(p => p.PermissionRoles)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null)
            {
                return NotFound();
            }

            // Xóa tất cả các PermissionRole liên quan trước
            _context.PermissionRoles.RemoveRange(permission.PermissionRoles);
            _context.Permissions.Remove(permission);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
