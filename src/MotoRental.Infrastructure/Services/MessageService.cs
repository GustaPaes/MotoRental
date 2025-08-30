using Microsoft.Extensions.Logging;
using MotoRental.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MotoRental.Infrastructure.Services
{
    public class MessageService : IMessageService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IConnection connection, ILogger<MessageService> logger)
        {
            _connection = connection;
            _channel = _connection.CreateModel();
            _logger = logger;
        }

        public async Task PublishMessage<T>(T message, string queueName)
        {
            try
            {
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation("Message published to queue: {QueueName}", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to queue: {QueueName}", queueName);
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
