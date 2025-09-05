using AutoMapper;
using Moq;
using MotoRental.Application.DTOs;
using MotoRental.Application.Services;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Interfaces;
using Xunit;

namespace MotoRental.Test.Application.Services
{
    public class RentalServiceTests
    {
        private readonly Mock<IRentalRepository> _rentalRepositoryMock;
        private readonly Mock<IMotorcycleRepository> _motorcycleRepositoryMock;
        private readonly Mock<IDeliveryPersonRepository> _deliveryPersonRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly RentalService _service;

        public RentalServiceTests()
        {
            _rentalRepositoryMock = new Mock<IRentalRepository>();
            _motorcycleRepositoryMock = new Mock<IMotorcycleRepository>();
            _deliveryPersonRepositoryMock = new Mock<IDeliveryPersonRepository>();
            _mapperMock = new Mock<IMapper>();
            _service = new RentalService(
                _rentalRepositoryMock.Object,
                _motorcycleRepositoryMock.Object,
                _deliveryPersonRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldReturnGuid()
        {
            // Arrange
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);
            var endDate = tomorrow.AddDays(7); // 7-day plan

            var dto = new RentalCreateDTO
            {
                DeliveryPersonId = Guid.NewGuid(),
                MotorcycleId = Guid.NewGuid(),
                StartDate = tomorrow,
                EndDate = endDate,
                ExpectedEndDate = endDate,
                Plan = 7
            };

            var deliveryPerson = new DeliveryPerson { CnhType = "A" };
            var motorcycle = new Motorcycle();
            var rental = new Rental { Id = Guid.NewGuid() };

            _deliveryPersonRepositoryMock.Setup(r => r.GetByIdAsync(dto.DeliveryPersonId)).ReturnsAsync(deliveryPerson);
            _motorcycleRepositoryMock.Setup(r => r.GetByIdAsync(dto.MotorcycleId)).ReturnsAsync(motorcycle);
            _rentalRepositoryMock.Setup(r => r.GetActiveRentalsByMotorcycleIdAsync(dto.MotorcycleId, dto.StartDate, dto.EndDate))
                .ReturnsAsync(new List<Rental>());
            _rentalRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Rental>())).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<Rental>(dto)).Returns(rental);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.Equal(rental.Id, result);
            _rentalRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Rental>()), Times.Once);
        }

        [Fact]
        public async Task CalculateReturnCostAsync_EarlyReturn7DayPlan_ShouldCalculateCorrectly()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.Date.AddDays(-5);
            var endDate = startDate.AddDays(7);
            var actualEndDate = startDate.AddDays(5); // 2 days early

            var rental = new Rental
            {
                Id = rentalId,
                StartDate = startDate,
                EndDate = endDate,
                OriginalTotalCost = 210.00m, // 7 days * 30.00
                Status = RentalStatus.Active
            };

            _rentalRepositoryMock.Setup(r => r.GetByIdAsync(rentalId)).ReturnsAsync(rental);

            // Act
            var result = await _service.CalculateReturnCostAsync(rentalId, actualEndDate);

            // Assert
            decimal expectedBaseCost = 5 * 30.00m; // 5 days used
            decimal expectedAdditionalCost = 2 * 30.00m * 0.2m; // 20% of 2 unused days
            Assert.Equal(expectedBaseCost + expectedAdditionalCost, result.TotalCost);
        }
    }
}