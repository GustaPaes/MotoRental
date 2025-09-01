using MotoRental.Application.DTOs.Rental;
using MotoRental.Domain.Entities;

namespace MotoRental.Application.Interfaces
{
    public interface IRentalService
    {
        Task<decimal> CalculateRentalCost(Guid rentalId, DateTime returnDate);
        Task<bool> CanRentMotorcycle(Guid deliveryPersonId);
        Task<Rental> CreateRentalAsync(CreateRentalRequest request);
        Task<RentalResponse> GetRentalByIdAsync(Guid id);
        Task<CalculateReturnResponse> CalculateReturnCostAsync(Guid rentalId, DateTime returnDate);
        Task<decimal> FinalizeRentalAsync(Guid rentalId, DateTime returnDate);
    }
}