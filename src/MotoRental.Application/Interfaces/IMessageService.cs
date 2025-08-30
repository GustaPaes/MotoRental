namespace MotoRental.Application.Interfaces
{
    public interface IMessageService
    {
        Task PublishMessage<T>(T message, string queueName);
    }
}
