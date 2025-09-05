using AutoMapper;
using Moq;
using MotoRental.Application.DTOs;
using MotoRental.Application.Services;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Interfaces;
using Xunit;

namespace MotoRental.Test.Application.Services
{
    public class MotorcycleServiceTests
    {
        private readonly Mock<IMotorcycleRepository> _motorcycleRepositoryMock;
        private readonly Mock<IRentalRepository> _rentalRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly MotorcycleService _service;

        public MotorcycleServiceTests()
        {
            _motorcycleRepositoryMock = new Mock<IMotorcycleRepository>();
            _rentalRepositoryMock = new Mock<IRentalRepository>();
            _mapperMock = new Mock<IMapper>();
            _service = new MotorcycleService(
                _motorcycleRepositoryMock.Object,
                _rentalRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WithUniqueLicensePlate_ShouldReturnGuid()
        {
            // Arrange
            var dto = new MotorcycleCreateDTO
            {
                Year = 2024,
                Model = "Honda CB 500",
                LicensePlate = "ABC1234"
            };

            var entity = new Motorcycle
            {
                Id = Guid.NewGuid(),
                Year = dto.Year,
                Model = dto.Model,
                LicensePlate = dto.LicensePlate
            };

            _motorcycleRepositoryMock.Setup(r => r.ExistsByLicensePlateAsync(dto.LicensePlate, null)).ReturnsAsync(false);
            _motorcycleRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Motorcycle>())).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<Motorcycle>(dto)).Returns(entity);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.Equal(entity.Id, result);
            _motorcycleRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Motorcycle>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNoActiveRentals_ShouldSucceed()
        {
            // Arrange
            var motorcycleId = Guid.NewGuid();
            _rentalRepositoryMock.Setup(r => r.HasActiveRentalsAsync(motorcycleId)).ReturnsAsync(false);
            _motorcycleRepositoryMock.Setup(r => r.DeleteAsync(motorcycleId)).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(motorcycleId);

            // Assert
            _motorcycleRepositoryMock.Verify(r => r.DeleteAsync(motorcycleId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithActiveRentals_ShouldThrowException()
        {
            // Arrange
            var motorcycleId = Guid.NewGuid();
            _rentalRepositoryMock.Setup(r => r.HasActiveRentalsAsync(motorcycleId)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(motorcycleId));
        }
    }
}