using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MotoRental.API.Controllers;
using MotoRental.Application.Interfaces;
using MotoRental.Domain.Entities;
using MotoRental.Infrastructure.Data;
using System.Linq.Expressions;
using Xunit;

namespace MotoRental.Test.API.Controllers
{
    public class MotorcyclesControllerTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly Mock<IMessageService> _mockMessageService;
        private readonly Mock<ILogger<MotorcyclesController>> _mockLogger;
        private readonly MotorcyclesController _controller;

        public MotorcyclesControllerTests()
        {
            _mockContext = new Mock<ApplicationDbContext>();
            _mockMessageService = new Mock<IMessageService>();
            _mockLogger = new Mock<ILogger<MotorcyclesController>>();

            // Mock DbSet for Motorcycles
            var motorcycles = new List<Motorcycle>
            {
                new () { Id = Guid.NewGuid(), Year = 2023, Model = "Honda CG 160", LicensePlate = "ABC1234" },
                new () { Id = Guid.NewGuid(), Year = 2024, Model = "Yamaha Factor 150", LicensePlate = "XYZ5678" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Motorcycle>>();
            mockSet.As<IQueryable<Motorcycle>>().Setup(m => m.Provider).Returns(motorcycles.Provider);
            mockSet.As<IQueryable<Motorcycle>>().Setup(m => m.Expression).Returns(motorcycles.Expression);
            mockSet.As<IQueryable<Motorcycle>>().Setup(m => m.ElementType).Returns(motorcycles.ElementType);
            mockSet.As<IQueryable<Motorcycle>>().Setup(m => m.GetEnumerator()).Returns(motorcycles.GetEnumerator());

            // Configurar métodos assíncronos
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] ids) => motorcycles.FirstOrDefault(m => m.Id == (Guid)ids[0]));

            _mockContext.Setup(c => c.Motorcycles).Returns(mockSet.Object);

            _controller = new MotorcyclesController(
                _mockContext.Object,
                _mockMessageService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetMotorcycles_ReturnsAllMotorcycles()
        {
            // Act
            var result = await _controller.GetMotorcycles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<Motorcycle>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetMotorcycles_WithFilter_ReturnsFilteredMotorcycles()
        {
            // Act
            var result = await _controller.GetMotorcycles("ABC");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<Motorcycle>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal("ABC1234", returnValue[0].LicensePlate);
        }

        [Fact]
        public async Task GetMotorcycle_WithValidId_ReturnsMotorcycle()
        {
            // Arrange
            var motorcycles = new List<Motorcycle>
            {
                new Motorcycle { Id = Guid.NewGuid(), Year = 2023, Model = "Honda CG 160", LicensePlate = "ABC1234" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Motorcycle>>();
            mockSet.As<IQueryable<Motorcycle>>().Setup(m => m.Provider).Returns(motorcycles.Provider);
            mockSet.As<IQueryable<Motorcycle>>().Setup(m => m.Expression).Returns(motorcycles.Expression);
            mockSet.As<IQueryable<Motorcycle>>().Setup(m => m.ElementType).Returns(motorcycles.ElementType);
            mockSet.As<IQueryable<Motorcycle>>().Setup(m => m.GetEnumerator()).Returns(motorcycles.GetEnumerator());

            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .ReturnsAsync((object[] ids) => motorcycles.FirstOrDefault(m => m.Id == (Guid)ids[0]));

            _mockContext.Setup(c => c.Motorcycles).Returns(mockSet.Object);

            var controller = new MotorcyclesController(
                _mockContext.Object,
                _mockMessageService.Object,
                _mockLogger.Object
            );

            var motorcycleId = motorcycles.First().Id;

            // Act
            var result = await controller.GetMotorcycle(motorcycleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Motorcycle>(okResult.Value);
            Assert.Equal(motorcycleId, returnValue.Id);
        }

        [Fact]
        public async Task CreateMotorcycle_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var motorcycle = new Motorcycle
            {
                Year = 2023,
                Model = "Honda Biz 125",
                LicensePlate = "DEF9012"
            };

            var mockSet = new Mock<DbSet<Motorcycle>>();
            mockSet.Setup(m => m.AnyAsync(It.IsAny<Expression<Func<Motorcycle, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockContext.Setup(c => c.Motorcycles).Returns(mockSet.Object);
            _mockContext.Setup(c => c.Set<Motorcycle>()).Returns(mockSet.Object);

            // Act
            var result = await _controller.CreateMotorcycle(motorcycle);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(motorcycle, createdResult.Value);
        }
    }
}