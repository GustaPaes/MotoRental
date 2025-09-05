using Microsoft.Extensions.Logging;
using MotoRental.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MotoRental.Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly IConnection _connection;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IConnection connection, ILogger<MessageService> logger)
        {
            _connection = connection;
            _logger = logger;

            // Verificar se a conexão está aberta
            if (!_connection.IsOpen)
            {
                _logger.LogWarning("Conexão RabbitMQ não está aberta ao criar MessageService");
            }
        }

        public async Task PublishMessage<T>(T message, string queueName)
        {
            // Verificar se a conexão está válida antes de usar
            if (_connection == null || !_connection.IsOpen)
            {
                _logger.LogError("Conexão RabbitMQ não está disponível");
                throw new InvalidOperationException("Conexão RabbitMQ não está disponível");
            }

            IModel channel = null;
            try
            {
                // Criar um novo canal para cada operação
                channel = _connection.CreateModel();

                // Declarar a fila
                channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                // Serializar a mensagem
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                // Configurar propriedades da mensagem
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                // Publicar a mensagem
                channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation("Mensagem publicada na fila: {QueueName}", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar mensagem na fila: {QueueName}", queueName);
                throw;
            }
            finally
            {
                channel?.Close();
                channel?.Dispose();
            }
        }
    }
}