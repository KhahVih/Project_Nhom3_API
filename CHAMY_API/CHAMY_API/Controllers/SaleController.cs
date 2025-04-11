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
    public class SaleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public SaleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get sale 
        [HttpGet("GetSale")]
        public async Task<ActionResult<IEnumerable<SaleDTO>>> GetSale()
        {
            var query = _context.Sale
                .Select(s => new SaleDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    DiscountPercentage = s.DiscountPercentage,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    IsActive = s.IsActive,

                }).ToList();

            return Ok(query);
        }

        // POST: api/sales
        [HttpPost]
        public async Task<ActionResult<SaleDTO>> CreateSale([FromBody] SaleDTO saleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Map DTO sang entity
                var sale = new Sale
                {
                    Name = saleDto.Name,
                    DiscountPercentage = saleDto.DiscountPercentage,
                    StartDate = saleDto.StartDate,
                    EndDate = saleDto.EndDate,
                    IsActive = true,
                };

                _context.Sale.Add(sale);
                await _context.SaveChangesAsync();

                // Map entity về DTO để trả về
                saleDto.Id = sale.Id;
                return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, saleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // PUT: api/sales/id
        [HttpPut("{id}")]
        public async Task<ActionResult<SaleDTO>> UpdateSale(int id, SaleDTO saleDTO)
        {
            try
            {
                var sale = await _context.Sale.FindAsync(id);

                if (sale == null)
                {
                    return NotFound($"Sale with ID {id} not found");
                }

                // Cập nhật
                sale.Name = saleDTO.Name;
                sale.DiscountPercentage = saleDTO.DiscountPercentage;
                sale.StartDate = saleDTO.StartDate;
                sale.EndDate = saleDTO.EndDate;
                _context.Entry(sale).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Map sang DTO để trả về
                var saleDto = new SaleDTO
                {
                    Id = sale.Id,
                    Name = sale.Name,
                    DiscountPercentage = sale.DiscountPercentage,
                    StartDate = sale.StartDate,
                    EndDate = sale.EndDate,
                    IsActive = sale.IsActive
                };

                return Ok(saleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/sales/5/isactive
        [HttpPut("{id}/isactive")]
        public async Task<ActionResult<SaleDTO>> UpdateSaleStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var sale = await _context.Sale.FindAsync(id);

                if (sale == null)
                {
                    return NotFound($"Sale with ID {id} not found");
                }

                // Cập nhật trạng thái
                sale.IsActive = isActive;
                _context.Entry(sale).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Map sang DTO để trả về
                var saleDto = new SaleDTO
                {
                    Id = sale.Id,
                    Name = sale.Name,
                    DiscountPercentage = sale.DiscountPercentage,
                    StartDate = sale.StartDate,
                    EndDate = sale.EndDate,
                    IsActive = sale.IsActive
                };

                return Ok(saleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET helper method
        [HttpGet("{id}")]
        public async Task<ActionResult<SaleDTO>> GetSale(int id)
        {
            var sale = await _context.Sale.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            // Map sang DTO
            var saleDto = new SaleDTO
            {
                Id = sale.Id,
                Name = sale.Name,
                DiscountPercentage = sale.DiscountPercentage,
                StartDate = sale.StartDate,
                EndDate = sale.EndDate,
                IsActive = sale.IsActive
            };

            return Ok(saleDto);
        }
    }
}

