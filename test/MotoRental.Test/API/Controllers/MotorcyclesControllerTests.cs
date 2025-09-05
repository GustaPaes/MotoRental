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
    public class MotorcyclesControllerTests
    {
        private readonly Mock<IMotorcycleService> _serviceMock;
        private readonly Mock<IMessageService> _messageServiceMock;
        private readonly Mock<ILogger<MotorcyclesController>> _loggerMock;
        private readonly MotorcyclesController _controller;

        public MotorcyclesControllerTests()
        {
            _serviceMock = new Mock<IMotorcycleService>();
            _messageServiceMock = new Mock<IMessageService>();
            _loggerMock = new Mock<ILogger<MotorcyclesController>>();
            _controller = new MotorcyclesController(
                _serviceMock.Object,
                _messageServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetMotorcycles_WithLicensePlateFilter_ShouldReturnFilteredResults()
        {
            // Arrange
            var licensePlate = "ABC1234";
            var motorcycles = new List<MotorcycleResponseDTO>
            {
                new MotorcycleResponseDTO
                {
                    Id = Guid.NewGuid(),
                    Year = 2024,
                    Model = "Honda CB 500",
                    LicensePlate = licensePlate
                }
            };

            _serviceMock.Setup(s => s.GetAllAsync(licensePlate)).ReturnsAsync(motorcycles);

            // Act
            var result = await _controller.GetMotorcycles(licensePlate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMotorcycles = Assert.IsAssignableFrom<IEnumerable<MotorcycleResponse>>(okResult.Value);
            Assert.Single(returnedMotorcycles);
            Assert.Equal(licensePlate, returnedMotorcycles.First().LicensePlate);
        }
    }
}