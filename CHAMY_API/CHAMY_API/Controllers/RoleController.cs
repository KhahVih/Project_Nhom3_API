using CHAMY_API.Models.DTO;
using CHAMY_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using CHAMY_API.Data;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Role - Lấy danh sách vai trò
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new RoleDTO { Id = r.Id, Name = r.Name })
                .ToListAsync();
            return Ok(roles);
        }

        // GET: api/Role/5 - Lấy thông tin một vai trò
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDTO>> GetRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(new RoleDTO { Id = role.Id, Name = role.Name });
        }

        // POST: api/Role - Thêm vai trò mới
        [HttpPost]
        public async Task<ActionResult<RoleDTO>> CreateRole(RoleDTO roleDto)
        {
            var role = new Role { Name = roleDto.Name };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            roleDto.Id = role.Id;
            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, roleDto);
        }

        // PUT: api/Role/5 - Sửa thông tin vai trò
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, RoleDTO roleDto)
        {
            if (id != roleDto.Id)
            {
                return BadRequest();
            }
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            role.Name = roleDto.Name;
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Role/5 - Xóa một vai trò
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
