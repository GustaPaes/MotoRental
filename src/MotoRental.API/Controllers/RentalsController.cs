using Microsoft.AspNetCore.Mvc;
using MotoRental.API.DTOs;
using MotoRental.Application.Interfaces;

namespace MotoRental.API.Controllers
{
    [ApiController]
    [Route("locacao")]
    public class RentalsController(
        IRentalService rentalService,
        ILogger<RentalsController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateLocacao([FromBody] RentalCreateDto request)
        {
            try
            {
                var rentalCreateDTO = new Application.DTOs.RentalCreateDTO
                {
                    DeliveryPersonId = request.DeliveryPersonId,
                    MotorcycleId = request.MotorcycleId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    ExpectedEndDate = request.ExpectedEndDate,
                    Plan = request.Plan
                };

                var rentalId = await rentalService.CreateAsync(rentalCreateDTO);
                return StatusCode(201);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao criar locação");
                return StatusCode(500, new { mensagem = "Erro interno do servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocacao(Guid id)
        {
            try
            {
                var rental = await rentalService.GetByIdAsync(id);

                if (rental == null)
                    return NotFound(new { mensagem = "Locação não encontrada" });

                // Calcular valor diário médio
                int rentalDays = (rental.EndDate - rental.StartDate).Days;
                decimal dailyCost = rentalDays > 0 ? rental.TotalCost / rentalDays : 0;

                var response = new RentalResponseDto
                {
                    Id = rental.Id,
                    DailyCost = dailyCost,
                    DeliveryPersonId = rental.DeliveryPersonId,
                    MotorcycleId = rental.MotorcycleId,
                    StartDate = rental.StartDate,
                    EndDate = rental.EndDate,
                    ExpectedEndDate = rental.ExpectedEndDate,
                    ReturnDate = rental.ActualEndDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao obter locação com ID: {Id}", id);
                return StatusCode(500, new { mensagem = "Erro interno do servidor" });
            }
        }

        [HttpPut("{id}/devolucao")]
        public async Task<IActionResult> InformarDevolucao(Guid id, [FromBody] RentalUpdateDto request)
        {
            try
            {
                await rentalService.ProcessReturnAsync(id, request.ActualEndDate);

                var calculation = await rentalService.CalculateReturnCostAsync(id, request.ActualEndDate);

                return Ok(new
                {
                    mensagem = "Data de devolução informada com sucesso",
                    custo_total = calculation.TotalCost,
                    custo_base = calculation.BaseCost,
                    multa_adicional = calculation.AdditionalCost
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao informar devolução para locação com ID: {Id}", id);
                return StatusCode(500, new { mensagem = "Erro interno do servidor" });
            }
        }
    }
}