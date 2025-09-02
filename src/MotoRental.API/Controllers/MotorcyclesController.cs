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
    public class MotorcyclesController(
        ApplicationDbContext context,
        IMessageService messageService,
        ILogger<MotorcyclesController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMotorcycles([FromQuery] string licensePlate = null)
        {
            try
            {
                var query = context.Motorcycles.AsQueryable();

                if (!string.IsNullOrEmpty(licensePlate))
                    query = query.Where(m => m.LicensePlate.Contains(licensePlate));

                var motorcycles = await query.ToListAsync();
                return Ok(motorcycles);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao obter motocicletas");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMotorcycle(Guid id)
        {
            try
            {
                var motorcycle = await context.Motorcycles.FindAsync(id);

                if (motorcycle == null)
                    return NotFound();

                return Ok(motorcycle);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao obter motocicleta com ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMotorcycle([FromBody] Motorcycle motorcycle)
        {
            try
            {
                if (await context.Motorcycles.AnyAsync(m => m.LicensePlate == motorcycle.LicensePlate))
                    return Conflict("A placa já existe");

                motorcycle.Id = Guid.NewGuid();
                context.Motorcycles.Add(motorcycle);
                await context.SaveChangesAsync();

                // Publish event
                await messageService.PublishMessage(new MotorcycleCreatedEvent
                {
                    Id = motorcycle.Id,
                    Year = motorcycle.Year,
                    LicensePlate = motorcycle.LicensePlate,
                    CreatedAt = DateTime.UtcNow
                }, "motorcycle-created-queue");

                return CreatedAtAction(nameof(GetMotorcycle), new { id = motorcycle.Id }, motorcycle);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao criar motocicleta");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMotorcycle(Guid id, [FromBody] Motorcycle motorcycle)
        {
            try
            {
                if (id != motorcycle.Id)
                    return BadRequest("Incompatibilidade de ID");

                if (await context.Motorcycles.AnyAsync(m => m.LicensePlate == motorcycle.LicensePlate && m.Id != id))
                    return Conflict("A placa já existe");

                context.Entry(motorcycle).State = EntityState.Modified;
                await context.SaveChangesAsync();

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
                logger.LogError(ex, "Erro ao atualizar motocicleta com ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMotorcycle(Guid id)
        {
            try
            {
                var motorcycle = await context.Motorcycles.FindAsync(id);
                if (motorcycle == null)
                    return NotFound();

                // Check if motorcycle has rentals
                if (await context.Rentals.AnyAsync(r => r.MotorcycleId == id))
                    return BadRequest("Não é possível excluir motocicleta com histórico de aluguel");

                context.Motorcycles.Remove(motorcycle);
                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao excluir motocicleta com ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<bool> MotorcycleExists(Guid id)
        {
            return await context.Motorcycles.AnyAsync(e => e.Id == id);
        }
    }
}