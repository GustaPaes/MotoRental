using AutoMapper;
using MotoRental.Application.DTOs;
using MotoRental.Application.Interfaces;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Interfaces;

namespace MotoRental.Application.Services
{
    public class RentalService(
        IRentalRepository rentalRepository,
        IMotorcycleRepository motorcycleRepository,
        IDeliveryPersonRepository deliveryPersonRepository,
        IMapper mapper) : IRentalService
    {
        public async Task<RentalResponseDTO> GetByIdAsync(Guid id)
        {
            var rental = await rentalRepository.GetByIdAsync(id);
            return mapper.Map<RentalResponseDTO>(rental);
        }

        public async Task<Guid> CreateAsync(RentalCreateDTO rentalCreateDTO)
        {
            // Validar entregador
            var deliveryPerson = await deliveryPersonRepository.GetByIdAsync(rentalCreateDTO.DeliveryPersonId);
            if (deliveryPerson == null)
                throw new InvalidOperationException("Entregador não encontrado");

            if (deliveryPerson.CnhType != "A" && deliveryPerson.CnhType != "A+B")
                throw new InvalidOperationException("Entregador não habilitado na categoria A");

            // Validar moto
            var motorcycle = await motorcycleRepository.GetByIdAsync(rentalCreateDTO.MotorcycleId);
            if (motorcycle == null)
                throw new InvalidOperationException("Moto não encontrada");

            // Validar datas
            if (rentalCreateDTO.StartDate >= rentalCreateDTO.EndDate)
                throw new InvalidOperationException("Data de início deve ser anterior à data de término");

            DateTime expectedStartDate = DateTime.UtcNow.Date.AddDays(1);
            if (rentalCreateDTO.StartDate.Date != expectedStartDate)
                throw new InvalidOperationException("Data de início deve ser exatamente o dia seguinte à data de criação");

            int expectedDuration = rentalCreateDTO.Plan;
            int actualDuration = (rentalCreateDTO.EndDate - rentalCreateDTO.StartDate).Days;
            if (actualDuration != expectedDuration)
                throw new InvalidOperationException($"Duração da locação deve ser de {expectedDuration} dias para o plano selecionado");

            // Verificar disponibilidade da moto
            var existingRental = await rentalRepository.GetActiveRentalsByMotorcycleIdAsync(
                rentalCreateDTO.MotorcycleId, rentalCreateDTO.StartDate, rentalCreateDTO.EndDate);

            if (existingRental.Any())
                throw new InvalidOperationException("Moto já alugada neste período");

            // Calcular custo
            decimal dailyCost = rentalCreateDTO.Plan switch
            {
                7 => 30.00m,
                15 => 28.00m,
                30 => 22.00m,
                45 => 20.00m,
                50 => 18.00m,
                _ => throw new ArgumentException("Plano inválido")
            };

            decimal totalCost = dailyCost * actualDuration;

            // Criar locação
            var rental = mapper.Map<Rental>(rentalCreateDTO);
            rental.Id = Guid.NewGuid();
            rental.TotalCost = totalCost;
            rental.OriginalTotalCost = totalCost;
            rental.Status = RentalStatus.Active;
            rental.CreatedAt = DateTime.UtcNow;

            await rentalRepository.AddAsync(rental);
            return rental.Id;
        }

        public async Task<ReturnCalculationResultDTO> CalculateReturnCostAsync(Guid rentalId, DateTime actualEndDate)
        {
            var rental = await rentalRepository.GetByIdAsync(rentalId);
            if (rental == null)
                throw new InvalidOperationException("Locação não encontrada");

            if (rental.Status == RentalStatus.Completed)
                throw new InvalidOperationException("Locação já devolvida");

            if (actualEndDate < rental.StartDate)
                throw new InvalidOperationException("Data de devolução não pode ser anterior à data de início");

            int originalRentalDays = (rental.EndDate - rental.StartDate).Days;
            decimal dailyCost = originalRentalDays > 0 ? rental.OriginalTotalCost / originalRentalDays : 0;

            decimal baseCost;
            decimal additionalCost = 0;

            if (actualEndDate <= rental.EndDate)
            {
                int actualRentalDays = (actualEndDate - rental.StartDate).Days;
                baseCost = dailyCost * actualRentalDays;

                if (actualEndDate < rental.EndDate)
                {
                    int daysNotUsed = (rental.EndDate - actualEndDate).Days;
                    decimal unusedCost = dailyCost * daysNotUsed;

                    additionalCost = originalRentalDays switch
                    {
                        7 => unusedCost * 0.2m,
                        15 => unusedCost * 0.4m,
                        _ => 0
                    };
                }
                else
                {
                    baseCost = rental.OriginalTotalCost;
                }
            }
            else
            {
                baseCost = rental.OriginalTotalCost;
                int extraDays = (actualEndDate - rental.EndDate).Days;
                additionalCost = extraDays * 50.00m;
            }

            return new ReturnCalculationResultDTO
            {
                TotalCost = baseCost + additionalCost,
                BaseCost = baseCost,
                AdditionalCost = additionalCost
            };
        }

        public async Task ProcessReturnAsync(Guid rentalId, DateTime actualEndDate)
        {
            var rental = await rentalRepository.GetByIdAsync(rentalId);
            if (rental == null)
                throw new InvalidOperationException("Locação não encontrada");

            if (rental.Status == RentalStatus.Completed)
                throw new InvalidOperationException("Locação já devolvida");

            var calculation = await CalculateReturnCostAsync(rentalId, actualEndDate);

            rental.ActualEndDate = actualEndDate;
            rental.TotalCost = calculation.TotalCost;
            rental.Status = RentalStatus.Completed;
            rental.UpdatedAt = DateTime.UtcNow;

            await rentalRepository.UpdateAsync(rental);
        }
    }
}