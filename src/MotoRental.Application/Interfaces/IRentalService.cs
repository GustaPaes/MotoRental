using MotoRental.Application.DTOs;

namespace MotoRental.Application.Interfaces
{
    public interface IRentalService
    {
        Task<RentalResponseDTO> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(RentalCreateDTO rentalCreateDTO);
        Task<ReturnCalculationResultDTO> CalculateReturnCostAsync(Guid rentalId, DateTime actualEndDate);
        Task ProcessReturnAsync(Guid rentalId, DateTime actualEndDate);
    }
}