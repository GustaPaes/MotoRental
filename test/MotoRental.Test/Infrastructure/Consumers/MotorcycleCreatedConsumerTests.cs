using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MotoRental.Application.Interfaces;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Events;
using MotoRental.Infrastructure.Consumers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MotoRental.Test.Infrastructure.Consumers
{
    public class MotorcycleCreatedConsumerTests
    {
        private readonly Mock<IConnection> _mockConnection;
        private readonly Mock<IModel> _mockChannel;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<ILogger<MotorcycleCreatedConsumer>> _mockLogger;
        private readonly Mock<IMongoRepository<Notification>> _mockNotificationRepository;

        public MotorcycleCreatedConsumerTests()
        {
            _mockConnection = new Mock<IConnection>();
            _mockChannel = new Mock<IModel>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockLogger = new Mock<ILogger<MotorcycleCreatedConsumer>>();
            _mockNotificationRepository = new Mock<IMongoRepository<Notification>>();

            _mockConnection.Setup(c => c.CreateModel()).Returns(_mockChannel.Object);

            // Mock service scope and service provider
            var mockScope = new Mock<IServiceScope>();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();

            mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
            mockScopeFactory.Setup(s => s.CreateScope()).Returns(mockScope.Object);

            _mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockScopeFactory.Object);

            _mockServiceProvider.Setup(sp => sp.GetService(typeof(IMongoRepository<Notification>)))
                .Returns(_mockNotificationRepository.Object);
        }

        [Fact]
        public async Task ProcessMessage_With2024Motorcycle_CreatesNotification()
        {
            // Arrange
            var consumer = new MotorcycleCreatedConsumer(
                _mockConnection.Object,
                _mockServiceProvider.Object,
                _mockLogger.Object
            );

            var message = new MotorcycleCreatedEvent
            {
                Id = Guid.NewGuid(),
                Year = 2024,
                LicensePlate = "TEST2024",
                CreatedAt = DateTime.UtcNow
            };

            var messageJson = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageJson);

            // Create a mock event args
            var ea = new BasicDeliverEventArgs
            {
                Body = body,
                DeliveryTag = 1UL
            };

            // Act - simulate message processing by invoking the event handler directly
            await consumer.ProcessMessageAsync(messageJson, ea.DeliveryTag);

            // Assert
            _mockNotificationRepository.Verify(r => r.InsertOneAsync(It.Is<Notification>(n =>
                n.Message.Contains("2024") && n.Message.Contains("TEST2024"))), Times.Once);

            _mockChannel.Verify(c => c.BasicAck(1UL, false), Times.Once);
        }

        [Fact]
        public async Task ProcessMessage_WithNon2024Motorcycle_DoesNotCreateNotification()
        {
            // Arrange
            var consumer = new MotorcycleCreatedConsumer(
                _mockConnection.Object,
                _mockServiceProvider.Object,
                _mockLogger.Object
            );

            var message = new MotorcycleCreatedEvent
            {
                Id = Guid.NewGuid(),
                Year = 2023,
                LicensePlate = "TEST2023",
                CreatedAt = DateTime.UtcNow
            };

            var messageJson = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageJson);

            // Create a mock event args
            var ea = new BasicDeliverEventArgs
            {
                Body = body,
                DeliveryTag = 1UL
            };

            // Act - simulate message processing by invoking the event handler directly
            await consumer.ProcessMessageAsync(messageJson, ea.DeliveryTag);

            // Assert
            _mockNotificationRepository.Verify(r => r.InsertOneAsync(It.IsAny<Notification>()), Times.Never);
            _mockChannel.Verify(c => c.BasicAck(1UL, false), Times.Once);
        }

        [Fact]
        public async Task ProcessMessage_WithInvalidMessage_LogsError()
        {
            // Arrange
            var consumer = new MotorcycleCreatedConsumer(
                _mockConnection.Object,
                _mockServiceProvider.Object,
                _mockLogger.Object
            );

            var invalidMessage = "invalid json";

            // Act
            await consumer.ProcessMessageAsync(invalidMessage, 1UL);

            // Assert
            _mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error processing message")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}