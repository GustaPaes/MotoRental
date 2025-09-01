using Microsoft.EntityFrameworkCore;
using MotoRental.Application.DTOs.Rental;
using MotoRental.Application.Interfaces;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Enums;
using MotoRental.Infrastructure.Data;

namespace MotoRental.Application.Services
{
    public class RentalService(ApplicationDbContext context) : IRentalService
    {
        public async Task<bool> CanRentMotorcycle(Guid deliveryPersonId)
        {
            var deliveryPerson = await context.DeliveryPeople
                .FirstOrDefaultAsync(d => d.Id == deliveryPersonId);

            return deliveryPerson?.CnhType == CnhType.A || deliveryPerson?.CnhType == CnhType.AB;
        }

        public async Task<decimal> CalculateRentalCost(Guid rentalId, DateTime returnDate)
        {
            var rental = await context.Rentals
                .Include(r => r.Motorcycle)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null)
                throw new ArgumentException("Aluguel não encontrado");

            var planDays = (rental.ExpectedEndDate - rental.StartDate).Days;
            decimal dailyRate = planDays switch
            {
                7 => 30.00m,
                15 => 28.00m,
                30 => 22.00m,
                45 => 20.00m,
                50 => 18.00m,
                _ => throw new InvalidOperationException("Plano de aluguel inválido")
            };

            decimal baseCost = dailyRate * planDays;

            if (returnDate < rental.ExpectedEndDate)
            {
                int unusedDays = (rental.ExpectedEndDate - returnDate).Days;
                decimal unusedCost = dailyRate * unusedDays;

                // Correção no cálculo da multa
                decimal penaltyPercentage = planDays switch
                {
                    7 => 0.20m,
                    15 => 0.40m,
                    _ => 0.50m
                };

                decimal penalty = unusedCost * penaltyPercentage;
                return baseCost - unusedCost + penalty;
            }
            else if (returnDate > rental.ExpectedEndDate)
            {
                int extraDays = (returnDate - rental.ExpectedEndDate).Days;
                return baseCost + (extraDays * 50.00m);
            }

            return baseCost;
        }

        public async Task<Rental> CreateRentalAsync(CreateRentalRequest request)
        {
            // Verificar se entregador pode alugar
            if (!await CanRentMotorcycle(request.DeliveryPersonId))
                throw new InvalidOperationException("Entregador não está habilitado para alugar motos");

            // Verificar se moto existe
            var motorcycle = await context.Motorcycles.FindAsync(request.MotorcycleId);
            if (motorcycle == null)
                throw new ArgumentException("Moto não encontrada");

            // Verificar se moto já está alugada no período
            var existingRental = await context.Rentals
                .Where(r => r.MotorcycleId == request.MotorcycleId &&
                           (r.StartDate <= DateTime.Today.AddDays(request.PlanDays) && r.EndDate >= DateTime.Today))
                .FirstOrDefaultAsync();

            if (existingRental != null)
                throw new InvalidOperationException("Moto já está alugada no período selecionado");

            // Validar dias do plano
            if (!new[] { 7, 15, 30, 45, 50 }.Contains(request.PlanDays))
                throw new ArgumentException("Dias do plano inválidos");

            // Calcular datas e custo
            var startDate = DateTime.Today.AddDays(1);
            var expectedEndDate = startDate.AddDays(request.PlanDays);

            decimal dailyRate = request.PlanDays switch
            {
                7 => 30.00m,
                15 => 28.00m,
                30 => 22.00m,
                45 => 20.00m,
                50 => 18.00m,
                _ => throw new ArgumentException("Dias do plano inválidos")
            };

            decimal totalCost = dailyRate * request.PlanDays;

            var rental = new Rental
            {
                Id = Guid.NewGuid(),
                DeliveryPersonId = request.DeliveryPersonId,
                MotorcycleId = request.MotorcycleId,
                StartDate = startDate,
                ExpectedEndDate = expectedEndDate,
                EndDate = expectedEndDate,
                TotalCost = totalCost
            };

            context.Rentals.Add(rental);
            await context.SaveChangesAsync();

            return rental;
        }

        public async Task<RentalResponse> GetRentalByIdAsync(Guid id)
        {
            var rental = await context.Rentals
                .Include(r => r.Motorcycle)
                .Include(r => r.DeliveryPerson)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
                return null;

            return new RentalResponse
            {
                Id = rental.Id,
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
                ExpectedEndDate = rental.ExpectedEndDate,
                TotalCost = rental.TotalCost,
                MotorcycleId = rental.MotorcycleId,
                DeliveryPersonId = rental.DeliveryPersonId,
                MotorcycleModel = rental.Motorcycle.Model,
                MotorcycleLicensePlate = rental.Motorcycle.LicensePlate,
                DeliveryPersonName = rental.DeliveryPerson.Name
            };
        }

        public async Task<CalculateReturnResponse> CalculateReturnCostAsync(Guid rentalId, DateTime returnDate)
        {
            var cost = await CalculateRentalCost(rentalId, returnDate);
            var rental = await context.Rentals.FindAsync(rentalId);

            string breakdown;
            if (returnDate < rental.ExpectedEndDate)
            {
                int earlyDays = (rental.ExpectedEndDate - returnDate).Days;
                breakdown = $"Devolução antecipada em {earlyDays} dias com multa aplicada";
            }
            else if (returnDate > rental.ExpectedEndDate)
            {
                int extraDays = (returnDate - rental.ExpectedEndDate).Days;
                breakdown = $"Devolução tardia em {extraDays} dias com cobranças adicionais";
            }
            else
            {
                breakdown = "Devolução na data esperada sem cobranças adicionais";
            }

            return new CalculateReturnResponse
            {
                TotalCost = cost,
                CostBreakdown = breakdown
            };
        }

        public async Task<decimal> FinalizeRentalAsync(Guid rentalId, DateTime returnDate)
        {
            var rental = await context.Rentals.FindAsync(rentalId);
            if (rental == null)
                throw new ArgumentException("Aluguel não encontrado");

            var totalCost = await CalculateRentalCost(rentalId, returnDate);

            rental.EndDate = returnDate;
            rental.TotalCost = totalCost;

            await context.SaveChangesAsync();

            return totalCost;
        }
    }
}