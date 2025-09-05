using Microsoft.EntityFrameworkCore;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Interfaces;
using MotoRental.Infrastructure.Data;

namespace MotoRental.Infrastructure.Repositories
{
    public class MotorcycleRepository(ApplicationDbContext context) : IMotorcycleRepository
    {
        public async Task<Motorcycle> GetByIdAsync(Guid id)
        {
            return await context.Motorcycles.FindAsync(id);
        }

        public async Task<IEnumerable<Motorcycle>> GetAllAsync()
        {
            return await context.Motorcycles.ToListAsync();
        }

        public async Task<IEnumerable<Motorcycle>> GetByLicensePlateAsync(string licensePlate)
        {
            return await context.Motorcycles
                .Where(m => m.LicensePlate.Contains(licensePlate))
                .ToListAsync();
        }

        public async Task AddAsync(Motorcycle entity)
        {
            await context.Motorcycles.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Motorcycle entity)
        {
            context.Motorcycles.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await context.Motorcycles.FindAsync(id);
            if (entity != null)
            {
                context.Motorcycles.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByLicensePlateAsync(string licensePlate, Guid? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await context.Motorcycles
                    .AnyAsync(m => m.LicensePlate == licensePlate && m.Id != excludeId.Value);
            }

            return await context.Motorcycles
                .AnyAsync(m => m.LicensePlate == licensePlate);
        }

        public async Task<bool> HasRentalsAsync(Guid motorcycleId)
        {
            return await context.Rentals
                .AnyAsync(r => r.MotorcycleId == motorcycleId);
        }
    }
}