using Microsoft.EntityFrameworkCore;
using MotoRental.Application.DTOs.Rental;
using MotoRental.Application.Services;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Enums;
using MotoRental.Infrastructure.Data;
using Xunit;

namespace MotoRental.Test.Application.Services
{
    public class RentalServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly RentalService _rentalService;

        public RentalServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"RentalTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _rentalService = new RentalService(_context);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var deliveryPersonA = new DeliveryPerson
            {
                Id = Guid.NewGuid(),
                Name = "João Silva",
                Cnpj = "12345678000195",
                BirthDate = new DateTime(1990, 1, 1),
                CnhNumber = "123456789",
                CnhType = CnhType.A
            };

            var deliveryPersonB = new DeliveryPerson
            {
                Id = Guid.NewGuid(),
                Name = "Maria Santos",
                Cnpj = "98765432000195",
                BirthDate = new DateTime(1985, 5, 15),
                CnhNumber = "987654321",
                CnhType = CnhType.B
            };

            _context.DeliveryPeople.AddRange(deliveryPersonA, deliveryPersonB);

            var motorcycle = new Motorcycle
            {
                Id = Guid.NewGuid(),
                Year = 2023,
                Model = "Honda CG 160",
                LicensePlate = "ABC1234"
            };

            _context.Motorcycles.Add(motorcycle);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CanRentMotorcycle_WithValidCnhTypeA_ReturnsTrue()
        {
            // Arrange
            var deliveryPerson = _context.DeliveryPeople.First(d => d.CnhType == CnhType.A);

            // Act
            var result = await _rentalService.CanRentMotorcycle(deliveryPerson.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CanRentMotorcycle_WithInvalidCnhTypeB_ReturnsFalse()
        {
            // Arrange
            var deliveryPerson = _context.DeliveryPeople.First(d => d.CnhType == CnhType.B);

            // Act
            var result = await _rentalService.CanRentMotorcycle(deliveryPerson.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateRentalAsync_WithValidData_ReturnsRental()
        {
            // Arrange
            var deliveryPerson = _context.DeliveryPeople.First(d => d.CnhType == CnhType.A);
            var motorcycle = _context.Motorcycles.First();

            var request = new CreateRentalRequest
            {
                DeliveryPersonId = deliveryPerson.Id,
                MotorcycleId = motorcycle.Id,
                PlanDays = 7
            };

            // Act
            var result = await _rentalService.CreateRentalAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(deliveryPerson.Id, result.DeliveryPersonId);
            Assert.Equal(motorcycle.Id, result.MotorcycleId);
            Assert.Equal(7, (result.ExpectedEndDate - result.StartDate).Days);
        }

        [Fact]
        public async Task CreateRentalAsync_WithInvalidCnhType_ThrowsException()
        {
            // Arrange
            var deliveryPerson = _context.DeliveryPeople.First(d => d.CnhType == CnhType.B);
            var motorcycle = _context.Motorcycles.First();

            var request = new CreateRentalRequest
            {
                DeliveryPersonId = deliveryPerson.Id,
                MotorcycleId = motorcycle.Id,
                PlanDays = 7
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _rentalService.CreateRentalAsync(request));
        }

        [Fact]
        public async Task CalculateRentalCost_WithEarlyReturn_AppliesPenalty()
        {
            // Arrange
            var rental = new Rental
            {
                Id = Guid.NewGuid(),
                StartDate = DateTime.Today.AddDays(1),
                ExpectedEndDate = DateTime.Today.AddDays(8),
                EndDate = DateTime.Today.AddDays(8),
                TotalCost = 210.00m,
                MotorcycleId = _context.Motorcycles.First().Id,
                DeliveryPersonId = _context.DeliveryPeople.First(d => d.CnhType == CnhType.A).Id
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            var earlyReturnDate = DateTime.Today.AddDays(5);

            // Act
            var result = await _rentalService.CalculateRentalCost(rental.Id, earlyReturnDate);

            // Assert
            // Expected: 5 dias a 30/dia = 150 + multa de 20% em 2 dias não utilizados (60 * 0,2 = 12) = 162
            Assert.Equal(162.00m, result);
        }

        [Fact]
        public async Task CalculateRentalCost_WithLateReturn_AppliesAdditionalCharge()
        {
            // Arrange
            var rental = new Rental
            {
                Id = Guid.NewGuid(),
                StartDate = DateTime.Today.AddDays(1),
                ExpectedEndDate = DateTime.Today.AddDays(8),
                EndDate = DateTime.Today.AddDays(8),
                TotalCost = 210.00m,
                MotorcycleId = _context.Motorcycles.First().Id,
                DeliveryPersonId = _context.DeliveryPeople.First(d => d.CnhType == CnhType.A).Id
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            var lateReturnDate = DateTime.Today.AddDays(10);

            // Act
            var result = await _rentalService.CalculateRentalCost(rental.Id, lateReturnDate);

            // Assert
            // Expected: 210 + (2 * 50) = 310
            Assert.Equal(310.00m, result);
        }
    }
}