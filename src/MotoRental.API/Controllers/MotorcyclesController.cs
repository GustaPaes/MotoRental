using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Events;
using MotoRental.Application.Interfaces;
using MotoRental.Infrastructure.Data;

namespace MotoRental.API.Controllers
{
    [ApiController]
    [Route("api/motorcycles")]
    public class MotorcyclesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMessageService _messageService;
        private readonly ILogger<MotorcyclesController> _logger;

        public MotorcyclesController(
            ApplicationDbContext context,
            IMessageService messageService,
            ILogger<MotorcyclesController> logger)
        {
            _context = context;
            _messageService = messageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetMotorcycles([FromQuery] string licensePlate = null)
        {
            try
            {
                var query = _context.Motorcycles.AsQueryable();

                if (!string.IsNullOrEmpty(licensePlate))
                    query = query.Where(m => m.LicensePlate.Contains(licensePlate));

                var motorcycles = await query.ToListAsync();
                return Ok(motorcycles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting motorcycles");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMotorcycle(Guid id)
        {
            try
            {
                var motorcycle = await _context.Motorcycles.FindAsync(id);

                if (motorcycle == null)
                    return NotFound();

                return Ok(motorcycle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting motorcycle with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMotorcycle([FromBody] Motorcycle motorcycle)
        {
            try
            {
                if (await _context.Motorcycles.AnyAsync(m => m.LicensePlate == motorcycle.LicensePlate))
                    return Conflict("License plate already exists");

                motorcycle.Id = Guid.NewGuid();
                _context.Motorcycles.Add(motorcycle);
                await _context.SaveChangesAsync();

                // Publish event
                await _messageService.PublishMessage(new MotorcycleCreatedEvent
                {
                    Id = motorcycle.Id,
                    Year = motorcycle.Year,
                    LicensePlate = motorcycle.LicensePlate,
                    CreatedAt = DateTime.UtcNow
                }, "motorcycle-created");

                return CreatedAtAction(nameof(GetMotorcycle), new { id = motorcycle.Id }, motorcycle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating motorcycle");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMotorcycle(Guid id, [FromBody] Motorcycle motorcycle)
        {
            try
            {
                if (id != motorcycle.Id)
                    return BadRequest("ID mismatch");

                if (await _context.Motorcycles.AnyAsync(m => m.LicensePlate == motorcycle.LicensePlate && m.Id != id))
                    return Conflict("License plate already exists");

                _context.Entry(motorcycle).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await MotorcycleExists(id))
                    return NotFound();

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating motorcycle with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMotorcycle(Guid id)
        {
            try
            {
                var motorcycle = await _context.Motorcycles.FindAsync(id);
                if (motorcycle == null)
                    return NotFound();

                // Check if motorcycle has rentals
                if (await _context.Rentals.AnyAsync(r => r.MotorcycleId == id))
                    return BadRequest("Cannot delete motorcycle with rental history");

                _context.Motorcycles.Remove(motorcycle);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting motorcycle with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<bool> MotorcycleExists(Guid id)
        {
            return await _context.Motorcycles.AnyAsync(e => e.Id == id);
        }
    }
}