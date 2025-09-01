using System.ComponentModel.DataAnnotations;

namespace MotoRental.Application.DTOs.Rental
{
    public class CalculateReturnRequest
    {
        [Required(ErrorMessage = "Data de devolução é obrigatória")]
        public DateTime ReturnDate { get; set; }
    }
}