using Microsoft.EntityFrameworkCore;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Interfaces;
using MotoRental.Infrastructure.Data;

namespace MotoRental.Infrastructure.Repositories
{
    public class DeliveryPersonRepository(ApplicationDbContext context) : IDeliveryPersonRepository
    {
        public async Task<DeliveryPerson> GetByIdAsync(Guid id)
        {
            return await context.DeliveryPeople.FindAsync(id);
        }

        public async Task<IEnumerable<DeliveryPerson>> GetAllAsync()
        {
            return await context.DeliveryPeople.ToListAsync();
        }

        public async Task AddAsync(DeliveryPerson entity)
        {
            await context.DeliveryPeople.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DeliveryPerson entity)
        {
            context.DeliveryPeople.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await context.DeliveryPeople.FindAsync(id);
            if (entity != null)
            {
                context.DeliveryPeople.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByCnpjAsync(string cnpj)
        {
            return await context.DeliveryPeople.AnyAsync(d => d.Cnpj == cnpj);
        }

        public async Task<bool> ExistsByCnhNumberAsync(string cnhNumber)
        {
            return await context.DeliveryPeople.AnyAsync(d => d.CnhNumber == cnhNumber);
        }

        public async Task<DeliveryPerson> GetByCnpjAsync(string cnpj)
        {
            return await context.DeliveryPeople.FirstOrDefaultAsync(d => d.Cnpj == cnpj);
        }
    }
}