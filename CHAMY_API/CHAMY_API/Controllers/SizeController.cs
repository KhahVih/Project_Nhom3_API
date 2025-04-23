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
    public class SizeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SizeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Color
        [HttpPost]
        public async Task<IActionResult> CreateSize([FromBody] SizeDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Tên size name không được để trống.");
            }

            var size = new Size
            {
                Name = dto.Name
            };

            _context.Sizes.Add(size);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm size thành công",
                data = size
            });
        }

        // Optional: GET all colors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Size>>> GetSize()
        {
            return await _context.Sizes.ToListAsync();
        }
    }
}
