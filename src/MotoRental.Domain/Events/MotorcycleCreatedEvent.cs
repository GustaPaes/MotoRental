namespace MotoRental.Domain.Events
{
    public class MotorcycleCreatedEvent
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
