using AutoMapper;
using MotoRental.Application.DTOs;
using MotoRental.Application.Interfaces;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Interfaces;

namespace MotoRental.Application.Services
{
    public class MotorcycleService(
        IMotorcycleRepository motorcycleRepository,
        IRentalRepository rentalRepository,
        IMapper mapper) : IMotorcycleService
    {
        public async Task<MotorcycleResponseDTO> GetByIdAsync(Guid id)
        {
            var motorcycle = await motorcycleRepository.GetByIdAsync(id);
            return mapper.Map<MotorcycleResponseDTO>(motorcycle);
        }

        public async Task<IEnumerable<MotorcycleResponseDTO>> GetAllAsync(string licensePlate = null)
        {
            IEnumerable<Motorcycle> motorcycles;

            if (string.IsNullOrEmpty(licensePlate))
            {
                motorcycles = await motorcycleRepository.GetAllAsync();
            }
            else
            {
                motorcycles = await motorcycleRepository.GetByLicensePlateAsync(licensePlate);
            }

            return mapper.Map<IEnumerable<MotorcycleResponseDTO>>(motorcycles);
        }

        public async Task<Guid> CreateAsync(MotorcycleCreateDTO motorcycleCreateDTO)
        {
            if (await motorcycleRepository.ExistsByLicensePlateAsync(motorcycleCreateDTO.LicensePlate))
                throw new InvalidOperationException("Placa já existe");

            var motorcycle = mapper.Map<Motorcycle>(motorcycleCreateDTO);
            motorcycle.Id = Guid.NewGuid();

            await motorcycleRepository.AddAsync(motorcycle);
            return motorcycle.Id;
        }

        public async Task UpdateLicensePlateAsync(Guid id, string licensePlate)
        {
            if (await motorcycleRepository.ExistsByLicensePlateAsync(licensePlate, id))
                throw new InvalidOperationException("Placa já existe");

            var motorcycle = await motorcycleRepository.GetByIdAsync(id);
            motorcycle.LicensePlate = licensePlate;

            await motorcycleRepository.UpdateAsync(motorcycle);
        }

        public async Task DeleteAsync(Guid id)
        {
            if (await rentalRepository.HasActiveRentalsAsync(id))
                throw new InvalidOperationException("Não é possível excluir moto com locações ativas");

            await motorcycleRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsByLicensePlateAsync(string licensePlate, Guid? excludeId = null)
        {
            return await motorcycleRepository.ExistsByLicensePlateAsync(licensePlate, excludeId);
        }
    }
}