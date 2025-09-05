namespace MotoRental.Domain.Entities
{
    public class Rental
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public DateTime ActualEndDate { get; set; }
        public decimal TotalCost { get; set; }
        public decimal OriginalTotalCost { get; set; }
        public Guid MotorcycleId { get; set; }
        public Guid DeliveryPersonId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public RentalStatus Status { get; set; } = RentalStatus.Active;

        // Navigation properties
        public virtual Motorcycle Motorcycle { get; set; } = null!;
        public virtual DeliveryPerson DeliveryPerson { get; set; } = null!;
    }

    public enum RentalStatus
    {
        Active,
        Completed,
        Cancelled
    }
}
