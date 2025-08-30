namespace MotoRental.Application.Interfaces
{
    public interface IRentalService
    {
        Task<decimal> CalculateRentalCost(Guid rentalId, DateTime returnDate);
        Task<bool> CanRentMotorcycle(Guid deliveryPersonId);
    }
}
