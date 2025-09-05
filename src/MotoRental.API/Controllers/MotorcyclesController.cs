using Microsoft.AspNetCore.Mvc;
using MotoRental.API.DTOs;
using MotoRental.Application.Interfaces;

namespace MotoRental.API.Controllers
{
    [ApiController]
    [Route("motos")]
    public class MotorcyclesController(
        IMotorcycleService motorcycleService,
        IMessageService messageService,
        ILogger<MotorcyclesController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMotorcycles([FromQuery] string? placa)
        {
            try
            {
                var motorcycles = await motorcycleService.GetAllAsync(placa);

                // Mapear para DTO da API
                var response = motorcycles.Select(m => new MotorcycleResponse
                {
                    Id = m.Id.ToString(),
                    Year = m.Year,
                    Model = m.Model,
                    LicensePlate = m.LicensePlate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao obter motos");
                return StatusCode(500, new { mensagem = "Erro interno do servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMotorcycle(Guid id)
        {
            try
            {
                var motorcycle = await motorcycleService.GetByIdAsync(id);

                if (motorcycle == null)
                    return NotFound(new { mensagem = "Moto não encontrada" });

                var response = new MotorcycleResponse
                {
                    Id = motorcycle.Id.ToString(),
                    Year = motorcycle.Year,
                    Model = motorcycle.Model,
                    LicensePlate = motorcycle.LicensePlate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao obter moto com ID: {Id}", id);
                return StatusCode(500, new { mensagem = "Erro interno do servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMotorcycle([FromBody] MotorcycleCreateRequest request)
        {
            try
            {
                var motorcycleCreateDTO = new Application.DTOs.MotorcycleCreateDTO
                {
                    Year = request.Year,
                    Model = request.Model,
                    LicensePlate = request.LicensePlate
                };

                var motorcycleId = await motorcycleService.CreateAsync(motorcycleCreateDTO);

                // Publicar evento (se necessário mover para serviço)
                await messageService.PublishMessage(new Domain.Events.MotorcycleCreatedEvent
                {
                    Id = motorcycleId,
                    Year = request.Year,
                    LicensePlate = request.LicensePlate,
                    CreatedAt = DateTime.UtcNow
                }, "motorcycle-created-queue");

                return StatusCode(201);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao criar moto");
                return StatusCode(500, new { mensagem = "Erro interno do servidor" });
            }
        }

        [HttpPut("{id}/placa")]
        public async Task<IActionResult> UpdateLicensePlate(Guid id, [FromBody] LicensePlateUpdateRequest request)
        {
            try
            {
                await motorcycleService.UpdateLicensePlateAsync(id, request.LicensePlate);
                return Ok(new { mensagem = "Placa modificada com sucesso" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao atualizar placa da moto com ID: {Id}", id);
                return StatusCode(500, new { mensagem = "Erro interno do servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMotorcycle(Guid id)
        {
            try
            {
                await motorcycleService.DeleteAsync(id);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao excluir moto com ID: {Id}", id);
                return StatusCode(500, new { mensagem = "Erro interno do servidor" });
            }
        }
    }
}