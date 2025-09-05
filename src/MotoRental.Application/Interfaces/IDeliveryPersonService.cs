using MotoRental.Application.DTOs;

namespace MotoRental.Application.Interfaces
{
    public interface IDeliveryPersonService
    {
        Task<DeliveryPersonResponseDTO> GetByIdAsync(Guid id);
        Task<IEnumerable<DeliveryPersonResponseDTO>> GetAllAsync();
        Task<Guid> CreateAsync(DeliveryPersonCreateDTO deliveryPersonCreateDTO);
        Task UpdateCnhImageAsync(Guid id, string cnhImageUrl);
        Task<bool> CanRentAsync(Guid deliveryPersonId);
    }
}