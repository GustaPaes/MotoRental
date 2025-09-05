using MotoRental.Domain.Entities;

namespace MotoRental.Domain.Interfaces
{
    public interface IDeliveryPersonRepository : IRepository<DeliveryPerson>
    {
        Task<bool> ExistsByCnpjAsync(string cnpj);
        Task<bool> ExistsByCnhNumberAsync(string cnhNumber);
        Task<DeliveryPerson> GetByCnpjAsync(string cnpj);
    }
}