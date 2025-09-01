using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MotoRental.Application.DTOs.Rental;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Enums;
using MotoRental.Infrastructure.Data;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MotoRental.Test.API.Integration
{
    public class RentalsApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _context;

        public RentalsApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing context configuration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("RentalIntegrationTestDb");
                    });
                });
            });

            _client = _factory.CreateClient();

            // Create a scope to obtain a reference to the database context
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure the database is created
            _context.Database.EnsureCreated();

            // Seed the database with test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add delivery people
            var deliveryPerson = new DeliveryPerson
            {
                Id = Guid.NewGuid(),
                Name = "Test Delivery Person",
                Cnpj = "12345678000195",
                BirthDate = new DateTime(1990, 1, 1),
                CnhNumber = "123456789",
                CnhType = CnhType.A
            };

            _context.DeliveryPeople.Add(deliveryPerson);

            // Add motorcycles
            var motorcycle = new Motorcycle
            {
                Id = Guid.NewGuid(),
                Year = 2023,
                Model = "Test Motorcycle",
                LicensePlate = "TEST123"
            };

            _context.Motorcycles.Add(motorcycle);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _scope.Dispose();
        }

        [Fact]
        public async Task CreateRental_WithValidData_ReturnsCreated()
        {
            // Arrange
            var deliveryPerson = await _context.DeliveryPeople.FirstAsync();
            var motorcycle = await _context.Motorcycles.FirstAsync();

            var request = new CreateRentalRequest
            {
                DeliveryPersonId = deliveryPerson.Id,
                MotorcycleId = motorcycle.Id,
                PlanDays = 7
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/rentals", request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var rental = await response.Content.ReadFromJsonAsync<Rental>();
            Assert.NotNull(rental);
            Assert.Equal(deliveryPerson.Id, rental.DeliveryPersonId);
            Assert.Equal(motorcycle.Id, rental.MotorcycleId);
        }

        [Fact]
        public async Task GetRental_WithValidId_ReturnsRental()
        {
            // Arrange
            var deliveryPerson = await _context.DeliveryPeople.FirstAsync();
            var motorcycle = await _context.Motorcycles.FirstAsync();

            var rental = new Rental
            {
                Id = Guid.NewGuid(),
                DeliveryPersonId = deliveryPerson.Id,
                MotorcycleId = motorcycle.Id,
                StartDate = DateTime.Today.AddDays(1),
                ExpectedEndDate = DateTime.Today.AddDays(8),
                EndDate = DateTime.Today.AddDays(8),
                TotalCost = 210.00m
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/rentals/{rental.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<Rental>();
            Assert.NotNull(result);
            Assert.Equal(rental.Id, result.Id);
        }

        [Fact]
        public async Task CalculateReturn_WithValidData_ReturnsCost()
        {
            // Arrange
            var deliveryPerson = await _context.DeliveryPeople.FirstAsync();
            var motorcycle = await _context.Motorcycles.FirstAsync();

            var rental = new Rental
            {
                Id = Guid.NewGuid(),
                DeliveryPersonId = deliveryPerson.Id,
                MotorcycleId = motorcycle.Id,
                StartDate = DateTime.Today.AddDays(1),
                ExpectedEndDate = DateTime.Today.AddDays(8),
                EndDate = DateTime.Today.AddDays(8),
                TotalCost = 210.00m
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            var request = new CalculateReturnRequest
            {
                ReturnDate = DateTime.Today.AddDays(5)
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/rentals/{rental.Id}/calculate-return", request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<CalculateReturnResponse>();
            Assert.NotNull(result);
            Assert.Equal(162.00m, result.TotalCost); // 5 dias a 30/dia = 150 + multa de 20% em 2 dias não utilizados (60 * 0,2 = 12) = 162
        }
    }
}