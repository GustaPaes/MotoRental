using Microsoft.EntityFrameworkCore;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Interfaces;
using MotoRental.Infrastructure.Data;

namespace MotoRental.Infrastructure.Repositories
{
    public class RentalRepository(ApplicationDbContext context) : IRentalRepository
    {
        public async Task<Rental> GetByIdAsync(Guid id)
        {
            return await context.Rentals
                .Include(r => r.Motorcycle)
                .Include(r => r.DeliveryPerson)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Rental>> GetAllAsync()
        {
            return await context.Rentals
                .Include(r => r.Motorcycle)
                .Include(r => r.DeliveryPerson)
                .ToListAsync();
        }

        public async Task AddAsync(Rental entity)
        {
            await context.Rentals.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Rental entity)
        {
            context.Rentals.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await context.Rentals.FindAsync(id);
            if (entity != null)
            {
                context.Rentals.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Rental>> GetActiveRentalsByMotorcycleIdAsync(Guid motorcycleId, DateTime startDate, DateTime endDate)
        {
            return await context.Rentals
                .Where(r => r.MotorcycleId == motorcycleId &&
                           r.Status == RentalStatus.Active &&
                           (r.StartDate <= endDate && r.EndDate >= startDate))
                .ToListAsync();
        }

        public async Task<IEnumerable<Rental>> GetRentalsByDeliveryPersonIdAsync(Guid deliveryPersonId)
        {
            return await context.Rentals
                .Where(r => r.DeliveryPersonId == deliveryPersonId)
                .ToListAsync();
        }

        public async Task<bool> HasActiveRentalsAsync(Guid deliveryPersonId)
        {
            return await context.Rentals
                .AnyAsync(r => r.DeliveryPersonId == deliveryPersonId && r.Status == RentalStatus.Active);
        }
    }
}