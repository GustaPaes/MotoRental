using MotoRental.Application.DTOs;

namespace MotoRental.Application.Interfaces
{
    public interface IMotorcycleService
    {
        Task<MotorcycleResponseDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<MotorcycleResponseDTO>> GetAllAsync(string licensePlate = null);
        Task<Guid> CreateAsync(MotorcycleCreateDTO motorcycleCreateDTO);
        Task UpdateLicensePlateAsync(Guid id, string licensePlate);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsByLicensePlateAsync(string licensePlate, Guid? excludeId = null);
    }
}