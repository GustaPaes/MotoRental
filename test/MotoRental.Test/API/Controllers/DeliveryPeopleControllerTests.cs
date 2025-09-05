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
    public class DeliveryPeopleControllerTests
    {
        private readonly Mock<IDeliveryPersonService> _serviceMock;
        private readonly Mock<IStorageService> _storageServiceMock;
        private readonly Mock<ILogger<DeliveryPeopleController>> _loggerMock;
        private readonly DeliveryPeopleController _controller;

        public DeliveryPeopleControllerTests()
        {
            _serviceMock = new Mock<IDeliveryPersonService>();
            _storageServiceMock = new Mock<IStorageService>();
            _loggerMock = new Mock<ILogger<DeliveryPeopleController>>();
            _controller = new DeliveryPeopleController(
                _serviceMock.Object,
                _storageServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreateEntregador_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var request = new DeliveryPeopleCreateDto
            {
                Name = "John Doe",
                Cnpj = "12345678901234",
                BirthDate = new DateTime(1990, 1, 1),
                CnhNumber = "123456789",
                CnhType = "A"
            };

            var expectedId = Guid.NewGuid();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<DeliveryPersonCreateDTO>()))
                .ReturnsAsync(expectedId);

            // Act
            var result = await _controller.CreateEntregador(request);

            // Assert
            var createdAtResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdAtResult.StatusCode);
            _serviceMock.Verify(s => s.CreateAsync(It.IsAny<DeliveryPersonCreateDTO>()), Times.Once);
        }

        [Fact]
        public async Task CreateEntregador_WithInvalidOperation_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new DeliveryPeopleCreateDto();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<DeliveryPersonCreateDTO>()))
                .ThrowsAsync(new InvalidOperationException("Invalid data"));

            // Act
            var result = await _controller.CreateEntregador(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
    }
}