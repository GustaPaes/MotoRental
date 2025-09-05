using MotoRental.Domain.Entities;

namespace MotoRental.Domain.Interfaces
{
    public interface IRentalRepository : IRepository<Rental>
    {
        Task<IEnumerable<Rental>> GetActiveRentalsByMotorcycleIdAsync(Guid motorcycleId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Rental>> GetRentalsByDeliveryPersonIdAsync(Guid deliveryPersonId);
        Task<bool> HasActiveRentalsAsync(Guid deliveryPersonId);
    }
}