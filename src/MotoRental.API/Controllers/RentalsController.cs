using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoRental.Application.DTOs.Rental;
using MotoRental.Application.Interfaces;
using MotoRental.Infrastructure.Data;

namespace MotoRental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalsController(
        IRentalService rentalService,
        ApplicationDbContext context,
        ILogger<RentalsController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateRental([FromBody] CreateRentalRequest request)
        {
            try
            {
                var rental = await rentalService.CreateRentalAsync(request);
                return CreatedAtAction(nameof(GetRental), new { id = rental.Id }, rental);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Erro ao criar aluguel");
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Erro ao criar aluguel");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao criar aluguel");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRentals([FromQuery] Guid? deliveryPersonId = null, [FromQuery] Guid? motorcycleId = null)
        {
            try
            {
                var query = context.Rentals
                    .Include(r => r.Motorcycle)
                    .Include(r => r.DeliveryPerson)
                    .AsQueryable();

                if (deliveryPersonId.HasValue)
                    query = query.Where(r => r.DeliveryPersonId == deliveryPersonId.Value);

                if (motorcycleId.HasValue)
                    query = query.Where(r => r.MotorcycleId == motorcycleId.Value);

                var rentals = await query.ToListAsync();
                return Ok(rentals);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao obter aluguéis");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRental(Guid id)
        {
            try
            {
                var rental = await context.Rentals
                    .Include(r => r.Motorcycle)
                    .Include(r => r.DeliveryPerson)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rental == null)
                    return NotFound();

                return Ok(rental);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao obter aluguel com ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/calculate-return")]
        public async Task<IActionResult> CalculateReturn(Guid id, [FromBody] CalculateReturnRequest request)
        {
            try
            {
                var result = await rentalService.CalculateReturnCostAsync(id, request.ReturnDate);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Erro ao calcular devolução para o aluguel com ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Erro ao calcular devolução para o aluguel com ID: {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao calcular devolução para o aluguel com ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/return")]
        public async Task<IActionResult> ReturnMotorcycle(Guid id, [FromBody] CalculateReturnRequest request)
        {
            try
            {
                var totalCost = await rentalService.FinalizeRentalAsync(id, request.ReturnDate);
                return Ok(new { TotalCost = totalCost, Message = "Aluguel finalizado com sucesso" });
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Erro ao finalizar aluguel com ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Erro ao finalizar aluguel com ID: {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao finalizar aluguel com ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelRental(Guid id)
        {
            try
            {
                var rental = await context.Rentals.FindAsync(id);
                if (rental == null)
                    return NotFound();

                // Verificar se o aluguel já começou
                if (rental.StartDate <= DateTime.Today)
                    return BadRequest("Não é possível cancelar um aluguel que já iniciou");

                context.Rentals.Remove(rental);
                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao cancelar aluguel com ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}