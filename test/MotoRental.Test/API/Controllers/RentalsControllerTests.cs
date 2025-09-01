using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MotoRental.API.Controllers;
using MotoRental.Application.DTOs.Rental;
using MotoRental.Application.Interfaces;
using MotoRental.Domain.Entities;
using MotoRental.Infrastructure.Data;
using Xunit;

namespace MotoRental.Test.API.Controllers
{
    public class RentalsControllerTests
    {
        private readonly Mock<IRentalService> _mockRentalService;
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<ILogger<RentalsController>> _mockLogger;
        private readonly RentalsController _controller;

        public RentalsControllerTests()
        {
            _mockRentalService = new Mock<IRentalService>();
            _mockContext = CreateMockDbContext();
            _mockLogger = new Mock<ILogger<RentalsController>>();

            _controller = new RentalsController(
                _mockRentalService.Object,
                _mockContext.Object,
                _mockLogger.Object
            );
        }

        private static Mock<ApplicationDbContext> CreateMockDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            return new Mock<ApplicationDbContext>(options);
        }

        [Fact]
        public async Task CreateRental_WithValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var request = new CreateRentalRequest
            {
                DeliveryPersonId = Guid.NewGuid(),
                MotorcycleId = Guid.NewGuid(),
                PlanDays = 7
            };

            var rental = new Rental
            {
                Id = Guid.NewGuid(),
                DeliveryPersonId = request.DeliveryPersonId,
                MotorcycleId = request.MotorcycleId,
                StartDate = DateTime.Today.AddDays(1),
                ExpectedEndDate = DateTime.Today.AddDays(8),
                EndDate = DateTime.Today.AddDays(8),
                TotalCost = 210.00m
            };

            _mockRentalService.Setup(s => s.CreateRentalAsync(request))
                .ReturnsAsync(rental);

            // Act
            var result = await _controller.CreateRental(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(rental, createdResult.Value);
            Assert.Equal(nameof(_controller.GetRental), createdResult.ActionName);
        }

        [Fact]
        public async Task CreateRental_WithInvalidOperation_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateRentalRequest
            {
                DeliveryPersonId = Guid.NewGuid(),
                MotorcycleId = Guid.NewGuid(),
                PlanDays = 7
            };

            _mockRentalService.Setup(s => s.CreateRentalAsync(request))
                .ThrowsAsync(new InvalidOperationException("Invalid operation"));

            // Act
            var result = await _controller.CreateRental(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid operation", (badRequestResult.Value as dynamic)?.message);
        }

        [Fact]
        public async Task CalculateReturn_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var request = new CalculateReturnRequest
            {
                ReturnDate = DateTime.Today.AddDays(5)
            };

            var response = new CalculateReturnResponse
            {
                TotalCost = 162.00m,
                CostBreakdown = "Retorno antecipado em 2 dias com aplicação de penalidade"
            };

            _mockRentalService.Setup(s => s.CalculateReturnCostAsync(rentalId, request.ReturnDate))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CalculateReturn(rentalId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task ReturnMotorcycle_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var request = new CalculateReturnRequest
            {
                ReturnDate = DateTime.Today.AddDays(5)
            };

            _mockRentalService.Setup(s => s.FinalizeRentalAsync(rentalId, request.ReturnDate))
                .ReturnsAsync(162.00m);

            // Act
            var result = await _controller.ReturnMotorcycle(rentalId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(162.00m, (okResult.Value as dynamic)?.TotalCost);
        }
    }
}