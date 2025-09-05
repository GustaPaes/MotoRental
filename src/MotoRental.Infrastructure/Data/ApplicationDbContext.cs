using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MotoRental.Domain.Entities;

namespace MotoRental.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Motorcycle> Motorcycles { get; set; }
        public DbSet<DeliveryPerson> DeliveryPeople { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurações específicas para Motorcycles
            modelBuilder.Entity<Motorcycle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LicensePlate).IsRequired();
                entity.HasIndex(e => e.LicensePlate).IsUnique();
            });

            // Configurações específicas para DeliveryPeople
            modelBuilder.Entity<DeliveryPerson>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cnpj).IsRequired();
                entity.Property(e => e.CnhNumber).IsRequired();
                entity.HasIndex(e => e.Cnpj).IsUnique();
                entity.HasIndex(e => e.CnhNumber).IsUnique();
            });

            // Configurações específicas para Rentals
            modelBuilder.Entity<Rental>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(d => d.Motorcycle)
                    .WithMany()
                    .HasForeignKey(d => d.MotorcycleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.DeliveryPerson)
                    .WithMany()
                    .HasForeignKey(d => d.DeliveryPersonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }

    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
