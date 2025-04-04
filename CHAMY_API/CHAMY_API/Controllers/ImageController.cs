using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHAMY_API.Models;
using CHAMY_API.Models.DTO;
using CHAMY_API.Data;

namespace CHAMY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ImagesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/images
        [HttpGet("getAllImages")]
        public async Task<IActionResult> GetImages()
        {
            var images = await _context.Images
                .Select(i => new ImageDTO
                {
                    Id = i.Id,
                    Name = i.Name,
                    Link = i.Link
                })
                .ToListAsync();

            return Ok(images);
        }

        // POST: api/images/upload
        [HttpPost("uploadImages")]
        public async Task<ActionResult<List<ImageDTO>>> UploadImages([FromForm] List<IFormFile> files)
        {
            if (files == null || !files.Any())
            {
                return BadRequest("No files uploaded.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var uploadFolder = Path.Combine(_environment.WebRootPath, "Image");
            var imageDTOs = new List<ImageDTO>();
            var errors = new List<string>();

            // Tạo thư mục Image nếu chưa tồn tại
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    errors.Add($"File {file.FileName} is empty.");
                    continue; // Bỏ qua file rỗng
                }

                // Kiểm tra định dạng file
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    errors.Add($"File {file.FileName} has an invalid extension. Only .jpg, .jpeg, .png, and .gif are allowed.");
                    continue;
                }

                // Tạo tên file duy nhất
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadFolder, fileName);

                try
                {
                    // Lưu file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Tạo URL
                    var fileUrl = $"{Request.Scheme}://{Request.Host}/Image/{fileName}";

                    // Tạo đối tượng Image
                    var image = new Image
                    {
                        Name = file.FileName, // Tên gốc của file
                        Link = fileUrl
                    };

                    _context.Images.Add(image);
                    await _context.SaveChangesAsync();

                    // Thêm vào danh sách trả về
                    imageDTOs.Add(new ImageDTO
                    {
                        Id = image.Id,
                        Name = image.Name,
                        Link = image.Link
                    });
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing file {file.FileName}: {ex.Message}");
                }
            }

            // Kiểm tra kết quả
            if (imageDTOs.Count == 0)
            {
                return BadRequest(new { Message = "No valid files were processed.", Errors = errors });
            }

            if (errors.Any())
            {
                return Ok(new { Images = imageDTOs, Warnings = errors });
            }

            return Ok(imageDTOs); // Trả về danh sách ImageDTO
        }

        // GET: api/images/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ImageDTO>> GetImage(int id)
        {
            var image = await _context.Images.FindAsync(id);

            if (image == null)
            {
                return NotFound();
            }

            var imageDTO = new ImageDTO
            {
                Id = image.Id,
                Name = image.Name,
                Link = image.Link
            };

            return Ok(imageDTO);
        }

    }
}