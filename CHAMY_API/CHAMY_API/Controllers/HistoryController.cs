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
        public async Task<IActionResult> GetAllHistories(int page = 1)
        {
            const int pageSize = 12;
            if (page < 1) page = 1;
            var query = _context.History
                .Include(h => h.Customer) // Include thông tin Customer
                .OrderByDescending(h => h.CreatedAt);
            // tổng số lịch sử hoạt động  
            var totalHistory = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalHistory / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            var histories = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(h => new HistoryDTO
                {
                    CustomerId = h.CustomerId,
                    CustomerName = h.Customer.Fullname ?? h.Customer.Username, // Lấy Fullname hoặc Username nếu Fullname null
                    Action = h.Action,
                    Description = h.Description,
                    CreatedAt = h.CreatedAt,

                })
                .ToListAsync();
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang 
                ToTalHistories = totalHistory, // Tổng số khách hàng 
                Histories = histories, // danh sách khách hàng 
            };
            return Ok(result);
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
        public async Task<IActionResult> GetCustomerHistory(int customerId, int page = 1)
        {
            const int pageSize = 12;
            if (page < 1) page = 1;
            var query = _context.History
                .Where(h => h.CustomerId == customerId)
                .OrderByDescending(h => h.CreatedAt);
            // tổng số lịch sử hoạt động  
            var totalHistory = await query.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalHistory / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            var histories = await query
                .Select(h => new HistoryDTO
                {
                    CustomerId = h.CustomerId,
                    CustomerName = h.Customer.Fullname ?? h.Customer.Username, // Lấy Fullname hoặc Username nếu Fullname null
                    Action = h.Action,
                    Description = h.Description,
                    CreatedAt = h.CreatedAt,

                })
                .ToListAsync();
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang 
                ToTalHistories = totalHistory, // Tổng số khách hàng 
                Histories = histories, // danh sách khách hàng 
            };

            return Ok(result);
        }
    }
}
