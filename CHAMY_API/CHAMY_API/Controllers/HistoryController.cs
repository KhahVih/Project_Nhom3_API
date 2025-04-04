using CHAMY_API.Data;
using CHAMY_API.Models.DTO;
using CHAMY_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public HistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/history
        [HttpGet]
        public async Task<IActionResult> GetAllHistories()
        {
            var histories = await _context.History
                .Include(h => h.Customer) // Include thông tin Customer
                .Select(h => new HistoryDTO
                {
                    CustomerId = h.CustomerId,
                    CustomerName = h.Customer.Fullname ?? h.Customer.Username, // Lấy Fullname hoặc Username nếu Fullname null
                    Action = h.Action,
                    Description = h.Description,
                    CreatedAt = h.CreatedAt,

                })
                .ToListAsync();

            return Ok(histories);
        }

        [HttpPost]
        public async Task<IActionResult> AddHistory([FromBody] HistoryDTO historyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var history = new History
            {
                CustomerId = historyDto.CustomerId,
                Action = historyDto.Action,
                Description = historyDto.Description,
                CreatedAt = DateTime.UtcNow,
                //IpAddress = HttpContext.Request.Path + HttpContext.Request.QueryString // lấy đường  dẫn của trang 
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.History.Add(history);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã ghi lại lịch sử", HistoryId = history.Id });
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetCustomerHistory(int customerId)
        {
            var histories = await _context.History
                .Where(h => h.CustomerId == customerId)
                .Select(h => new
                {
                    h.Id,
                    h.Action,
                    h.Description,
                    h.CreatedAt,
                    h.IpAddress
                })
                .ToListAsync();

            return Ok(histories);
        }
    }
}
