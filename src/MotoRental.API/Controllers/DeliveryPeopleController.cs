using Microsoft.AspNetCore.Mvc;
using MotoRental.API.DTOs;
using MotoRental.Application.Interfaces;

namespace MotoRental.API.Controllers
{
    [ApiController]
    [Route("entregadores")]
    public class DeliveryPeopleController(
        IDeliveryPersonService deliveryPersonService,
        IStorageService storageService,
        ILogger<DeliveryPeopleController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateEntregador([FromBody] DeliveryPeopleCreateDto request)
        {
            try
            {
                var deliveryPersonCreateDTO = new Application.DTOs.DeliveryPersonCreateDTO
                {
                    Name = request.Name,
                    Cnpj = request.Cnpj,
                    BirthDate = request.BirthDate,
                    CnhNumber = request.CnhNumber,
                    CnhType = request.CnhType
                };

                var id = await deliveryPersonService.CreateAsync(deliveryPersonCreateDTO);
                return StatusCode(201, new { id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao criar entregador");
                return StatusCode(500, new { mensagem = "Erro interno do servidor" });
            }
        }

        [HttpPost("{id}/cnh")]
        public async Task<IActionResult> UploadCnhImage(Guid id, [FromBody] CnhImageUpdateDto request)
        {
            try
            {
                // Converter base64 e fazer upload
                var file = ConvertBase64ToIFormFile(request.ImagemCnh, "cnh_image");
                var imageUrl = await storageService.UploadImageAsync(file);

                await deliveryPersonService.UpdateCnhImageAsync(id, imageUrl);

                return StatusCode(201);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao fazer upload da imagem da CNH");
                return StatusCode(500, new { mensagem = "Erro interno do servidor" });
            }
        }

        private static FormFile ConvertBase64ToIFormFile(string base64String, string fileName)
        {
            // Remover cabeçalho se existir (ex: "data:image/png;base64,")
            var base64Data = base64String.Split(',').LastOrDefault() ?? base64String;
            byte[] bytes;

            try
            {
                bytes = Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("String base64 inválida.");
            }

            if (bytes.Length == 0)
            {
                throw new InvalidOperationException("Arquivo vazio.");
            }

            // Determinar o content type a partir dos bytes
            string contentType;
            if (bytes.Length >= 8 &&
                bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
            {
                contentType = "image/png";
                fileName += ".png";
            }
            else if (bytes.Length >= 2 && bytes[0] == 0x42 && bytes[1] == 0x4D)
            {
                contentType = "image/bmp";
                fileName += ".bmp";
            }
            else
            {
                throw new InvalidOperationException("Formato de imagem não suportado. Use PNG ou BMP.");
            }

            // Criar um stream a partir dos bytes
            var stream = new MemoryStream(bytes);

            // Criar o IFormFile
            var file = new FormFile(stream, 0, bytes.Length, "cnh_image", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            return file;
        }
    }
}