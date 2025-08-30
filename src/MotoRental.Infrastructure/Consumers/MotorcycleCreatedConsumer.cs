using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MotoRental.Application.Interfaces;
using MotoRental.Domain.Entities;
using MotoRental.Domain.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace MotoRental.Infrastructure.Consumers
{
    public class MotorcycleCreatedConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MotorcycleCreatedConsumer> _logger;
        private const string QueueName = "motorcycle-created";

        public MotorcycleCreatedConsumer(
            IConnection connection,
            IServiceProvider serviceProvider,
            ILogger<MotorcycleCreatedConsumer> logger)
        {
            _connection = connection;
            _channel = _connection.CreateModel();
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Declare the queue to ensure it exists
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                try
                {
                    var message = JsonSerializer.Deserialize<MotorcycleCreatedEvent>(messageJson);
                    _logger.LogInformation("Received message for motorcycle: {LicensePlate}", message?.LicensePlate);

                    if (message != null && message.Year == 2024)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var notificationRepository = scope.ServiceProvider.GetRequiredService<IMongoRepository<Notification>>();

                        var notification = new Notification
                        {
                            Message = $"Moto do ano 2024 cadastrada: Placa {message.LicensePlate}",
                            CreatedAt = DateTime.UtcNow,
                            EventType = "MotorcycleCreated",
                            EventData = new
                            {
                                message.Id,
                                message.LicensePlate,
                                message.Year,
                                message.CreatedAt
                            }
                        };

                        await notificationRepository.InsertOneAsync(notification);
                        _logger.LogInformation("Notification created for 2024 motorcycle: {LicensePlate}", message.LicensePlate);
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {Message}", messageJson);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}