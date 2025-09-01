using System.ComponentModel.DataAnnotations;

namespace MotoRental.Application.DTOs.Rental
{
    public class CreateRentalRequest
    {
        [Required(ErrorMessage = "ID do entregador é obrigatório")]
        public Guid DeliveryPersonId { get; set; }

        [Required(ErrorMessage = "ID da moto é obrigatório")]
        public Guid MotorcycleId { get; set; }

        [Required(ErrorMessage = "Dias do plano são obrigatórios")]
        [Range(7, 50, ErrorMessage = "Dias do plano devem ser 7, 15, 30, 45 ou 50")]
        public int PlanDays { get; set; }
    }
}
