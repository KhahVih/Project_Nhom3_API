using CHAMY_API.Data;
using CHAMY_API.Models;
using CHAMY_API.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ColorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Color
        [HttpPost]
        public async Task<IActionResult> CreateColor([FromBody] ColorDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Tên màu không được để trống.");
            }

            var color = new Color
            {
                Name = dto.Name
            };

            _context.Colors.Add(color);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm màu thành công",
                data = color
            });
        }

        // Optional: GET all colors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Color>>> GetColors()
        {
            return await _context.Colors.ToListAsync();
        }
    }
}
