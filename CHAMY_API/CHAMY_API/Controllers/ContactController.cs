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
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Contact
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactDTO>>> GetContacts()
        {
            var contacts = await _context.Contacts
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ContactDTO
                {
                    Id = c.Id,
                    Fullname = c.Fullname,
                    Address = c.Address,
                    Email = c.Email,
                    Phonenumber = c.Phonenumber,
                    Title = c.Title,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(contacts);
        }

        // GET: api/Contact/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ContactDTO>> GetContact(int id)
        {
            var contact = await _context.Contacts
                .Select(c => new ContactDTO
                {
                    Id = c.Id,
                    Fullname = c.Fullname,
                    Address = c.Address,
                    Email = c.Email,
                    Phonenumber = c.Phonenumber,
                    Title = c.Title,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact == null)
            {
                return NotFound();
            }

            return Ok(contact);
        }

        // POST: api/Contact
        [HttpPost]
        public async Task<ActionResult<ContactDTO>> CreateContact(ContactDTO contactDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contact = new Contact
            {
                Fullname = contactDTO.Fullname,
                Address = contactDTO.Address,
                Email = contactDTO.Email,
                Phonenumber = contactDTO.Phonenumber,
                Title = contactDTO.Title,
                Description = contactDTO.Description,
                CreatedAt = DateTime.Now // Tự động gán thời gian tạo
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            // Gán lại ID cho DTO sau khi lưu
            contactDTO.Id = contact.Id;
            contactDTO.CreatedAt = contact.CreatedAt;

            return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contactDTO);
        }

        // PUT: api/Contact/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContact(int id, ContactDTO contactDTO)
        {
            if (id != contactDTO.Id)
            {
                return BadRequest("ID mismatch");
            }

            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            // Cập nhật các trường
            contact.Fullname = contactDTO.Fullname;
            contact.Address = contactDTO.Address;
            contact.Email = contactDTO.Email;
            contact.Phonenumber = contactDTO.Phonenumber;
            contact.Title = contactDTO.Title;
            contact.Description = contactDTO.Description;
            // Không cập nhật CreatedAt vì đây là thời gian tạo ban đầu

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Contact/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
