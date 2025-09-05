using AutoMapper;
using Moq;
using MotoRental.Application.DTOs;
using MotoRental.Application.Services;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Interfaces;
using Xunit;

namespace MotoRental.Test.Application.Services
{
    public class DeliveryPersonServiceTests
    {
        private readonly Mock<IDeliveryPersonRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly DeliveryPersonService _service;

        public DeliveryPersonServiceTests()
        {
            _repositoryMock = new Mock<IDeliveryPersonRepository>();
            _mapperMock = new Mock<IMapper>();
            _service = new DeliveryPersonService(_repositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldReturnGuid()
        {
            // Arrange
            var dto = new DeliveryPersonCreateDTO
            {
                Name = "John Doe",
                Cnpj = "12345678901234",
                BirthDate = new DateTime(1990, 1, 1),
                CnhNumber = "123456789",
                CnhType = "A"
            };

            var entity = new DeliveryPerson
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Cnpj = dto.Cnpj,
                BirthDate = dto.BirthDate,
                CnhNumber = dto.CnhNumber,
                CnhType = dto.CnhType
            };

            _repositoryMock.Setup(r => r.ExistsByCnpjAsync(dto.Cnpj)).ReturnsAsync(false);
            _repositoryMock.Setup(r => r.ExistsByCnhNumberAsync(dto.CnhNumber)).ReturnsAsync(false);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<DeliveryPerson>())).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<DeliveryPerson>(dto)).Returns(entity);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.Equal(entity.Id, result);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<DeliveryPerson>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithExistingCnpj_ShouldThrowException()
        {
            // Arrange
            var dto = new DeliveryPersonCreateDTO { Cnpj = "12345678901234" };
            _repositoryMock.Setup(r => r.ExistsByCnpjAsync(dto.Cnpj)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CanRentAsync_WithValidCnhTypeA_ShouldReturnTrue()
        {
            // Arrange
            var deliveryPersonId = Guid.NewGuid();
            var deliveryPerson = new DeliveryPerson { CnhType = "A" };
            _repositoryMock.Setup(r => r.GetByIdAsync(deliveryPersonId)).ReturnsAsync(deliveryPerson);

            // Act
            var result = await _service.CanRentAsync(deliveryPersonId);

            // Assert
            Assert.True(result);
        }
    }
}