using Microsoft.EntityFrameworkCore;
using MotoRental.Application.Interfaces;
using MotoRental.Domain.Enums;
using MotoRental.Infrastructure.Data;

namespace MotoRental.Infrastructure.Services
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
                throw new ArgumentException("Rental not found");

            var planDays = (rental.ExpectedEndDate - rental.StartDate).Days;
            decimal dailyRate = planDays switch
            {
                7 => 30.00m,
                15 => 28.00m,
                30 => 22.00m,
                45 => 20.00m,
                50 => 18.00m,
                _ => throw new InvalidOperationException("Invalid rental plan")
            };

            decimal baseCost = dailyRate * planDays;

            if (returnDate < rental.ExpectedEndDate)
            {
                int unusedDays = (rental.ExpectedEndDate - returnDate).Days;
                decimal unusedCost = dailyRate * unusedDays;
                decimal penalty = planDays switch
                {
                    7 => unusedCost * 0.20m,
                    15 => unusedCost * 0.40m,
                    _ => unusedCost * 0.50m
                };

                return baseCost - unusedCost + penalty;
            }
            else if (returnDate > rental.ExpectedEndDate)
            {
                int extraDays = (returnDate - rental.ExpectedEndDate).Days;
                return baseCost + (extraDays * 50.00m);
            }

            return baseCost;
        }
    }
}
