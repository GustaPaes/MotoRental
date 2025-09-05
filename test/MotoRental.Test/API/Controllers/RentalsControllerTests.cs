using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MotoRental.API.Controllers;
using MotoRental.API.DTOs;
using MotoRental.Application.DTOs;
using MotoRental.Application.Interfaces;
using Xunit;

namespace MotoRental.Test.API.Controllers
{
    public class RentalsControllerTests
    {
        private readonly Mock<IRentalService> _serviceMock;
        private readonly Mock<ILogger<RentalsController>> _loggerMock;
        private readonly RentalsController _controller;

        public RentalsControllerTests()
        {
            _serviceMock = new Mock<IRentalService>();
            _loggerMock = new Mock<ILogger<RentalsController>>();
            _controller = new RentalsController(_serviceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateLocacao_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var request = new RentalCreateDto
            {
                DeliveryPersonId = Guid.NewGuid(),
                MotorcycleId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(8),
                ExpectedEndDate = DateTime.UtcNow.Date.AddDays(8),
                Plan = 7
            };

            var expectedId = Guid.NewGuid();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<RentalCreateDTO>()))
                .ReturnsAsync(expectedId);

            // Act
            var result = await _controller.CreateLocacao(request);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
        }
    }
}