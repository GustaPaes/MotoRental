namespace MotoRental.Application.DTOs.Rental
{
    public class RentalResponse
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public decimal TotalCost { get; set; }
        public Guid MotorcycleId { get; set; }
        public Guid DeliveryPersonId { get; set; }
        public string MotorcycleModel { get; set; } = string.Empty;
        public string MotorcycleLicensePlate { get; set; } = string.Empty;
        public string DeliveryPersonName { get; set; } = string.Empty;
    }
}