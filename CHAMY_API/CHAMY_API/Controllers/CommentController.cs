using CHAMY_API.Data;
using CHAMY_API.Models;
using CHAMY_API.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Comment?productId=1
        [HttpGet("GetAllComment/page={page}")]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetComments(int page = 1)
        {
            int pageSize = 8;
            if (page < 1) page = 1;
            var quey = _context.Comments
                .Include(c => c.Product)
                .Include(c => c.Customer);
            // tổng số liên hệ 
            var totalComment = await _context.Comments.CountAsync();
            // tính tổng số trang 
            var totalPage = (int)Math.Ceiling((double)totalComment / pageSize);
            // tính số bản ghi cần bỏ qua để đến bảng ghi 
            var skip = (page - 1) * pageSize;
            var comments = await quey
                .Skip(skip)
                .Take(pageSize)
                .Select(c => new CommentDTO
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    Vote = c.Vote,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    CustomerId = c.CustomerId,
                    IsShow = c.IsShow,
                    ProductName = c.Product.Name,
                    CustomerName = c.Customer.Fullname,
                })
                .ToListAsync();
            // Nếu có productId được truyền vào, lọc theo productId
            //if (productId.HasValue)
            //{
            //    query = query.Where(c => c.ProductId == productId.Value);
            //}
            // đối tượng chứa thông tin cần trả về 
            var result = new
            {
                CurrentPage = page,      // Số trang hiện tại
                TotalPages = totalPage, // Tổng số trang
                TotalComments = totalComment, // Tổng số bình luận 
                Comments = comments, // danh sách liên hệ 
            };
            return Ok(result);
        }

        // GET: api/Comment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDTO>> GetComment(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Product)
                .Include(c => c.Customer)
                .Select(c => new CommentDTO
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    Vote = c.Vote,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    CustomerId = c.CustomerId,
                    IsShow = c.IsShow,
                    ProductName = c.Product.Name,
                    CustomerName = c.Customer.Fullname,
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        // POST: api/Comment
        [HttpPost]
        public async Task<ActionResult<CommentDTO>> CreateComment(CommentDTO commentDTO)
        {
            if (commentDTO == null || commentDTO.ProductId <= 0)
            {
                return BadRequest("Comment data or ProductId is required.");
            }
            if (commentDTO.CustomerName == null || commentDTO.CustomerId <= 0) {
                return BadRequest("Comment data or CustomerId is required.");
            }

            var comment = new Comment
            {
                ProductId = commentDTO.ProductId,
                Vote = commentDTO.Vote,
                Description = commentDTO.Description,
                CreatedAt = DateTime.Now, // Tự động gán thời gian tạo
                CustomerId = commentDTO.CustomerId,
                IsShow = false // Mặc định là true nếu không cung cấp
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Cập nhật DTO với ID và CreatedAt từ database
            commentDTO.Id = comment.Id;
            commentDTO.CreatedAt = comment.CreatedAt;

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, commentDTO);
        }

        // PUT: api/Comment/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, CommentDTO commentDTO)
        {
            if (id != commentDTO.Id)
            {
                return BadRequest("ID mismatch");
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Cập nhật các trường
            comment.ProductId = commentDTO.ProductId;
            comment.Vote = commentDTO.Vote;
            comment.Description = commentDTO.Description;
            comment.CustomerId = commentDTO.CustomerId;
            comment.IsShow = commentDTO.IsShow;
            // CreatedAt không cập nhật vì là thời gian tạo ban đầu

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Comment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}

