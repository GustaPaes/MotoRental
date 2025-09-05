using AutoMapper;
using MotoRental.Application.DTOs;
using MotoRental.Application.Interfaces;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Interfaces;

namespace MotoRental.Application.Services
{
    public class DeliveryPersonService(IDeliveryPersonRepository deliveryPersonRepository, IMapper mapper) : IDeliveryPersonService
    {
        public async Task<DeliveryPersonResponseDTO> GetByIdAsync(Guid id)
        {
            var deliveryPerson = await deliveryPersonRepository.GetByIdAsync(id);
            return mapper.Map<DeliveryPersonResponseDTO>(deliveryPerson);
        }

        public async Task<IEnumerable<DeliveryPersonResponseDTO>> GetAllAsync()
        {
            var deliveryPeople = await deliveryPersonRepository.GetAllAsync();
            return mapper.Map<IEnumerable<DeliveryPersonResponseDTO>>(deliveryPeople);
        }

        public async Task<Guid> CreateAsync(DeliveryPersonCreateDTO deliveryPersonCreateDTO)
        {
            if (await deliveryPersonRepository.ExistsByCnpjAsync(deliveryPersonCreateDTO.Cnpj))
                throw new InvalidOperationException("CNPJ já existe");

            if (await deliveryPersonRepository.ExistsByCnhNumberAsync(deliveryPersonCreateDTO.CnhNumber))
                throw new InvalidOperationException("Número da CNH já existe");

            var deliveryPerson = mapper.Map<DeliveryPerson>(deliveryPersonCreateDTO);
            await deliveryPersonRepository.AddAsync(deliveryPerson);

            return deliveryPerson.Id;
        }

        public async Task UpdateCnhImageAsync(Guid id, string cnhImageUrl)
        {
            var deliveryPerson = await deliveryPersonRepository.GetByIdAsync(id);
            deliveryPerson.CnhImageUrl = cnhImageUrl;
            await deliveryPersonRepository.UpdateAsync(deliveryPerson);
        }

        public async Task<bool> CanRentAsync(Guid deliveryPersonId)
        {
            var deliveryPerson = await deliveryPersonRepository.GetByIdAsync(deliveryPersonId);
            return deliveryPerson.CnhType == "A" || deliveryPerson.CnhType == "A+B";
        }
    }
}