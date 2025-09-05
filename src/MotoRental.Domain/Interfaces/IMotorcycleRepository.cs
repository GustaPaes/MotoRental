using MotoRental.Domain.Entities;

namespace MotoRental.Domain.Interfaces
{
    public interface IMotorcycleRepository : IRepository<Motorcycle>
    {
        Task<bool> ExistsByLicensePlateAsync(string licensePlate, Guid? excludeId = null);
        Task<IEnumerable<Motorcycle>> GetByLicensePlateAsync(string licensePlate);
        Task<bool> HasRentalsAsync(Guid motorcycleId);
    }
}