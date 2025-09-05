namespace MotoRental.Application.DTOs
{
    public class RentalCreateDTO
    {
        public Guid DeliveryPersonId { get; set; }
        public Guid MotorcycleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public int Plan { get; set; }
    }

    public class RentalResponseDTO
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ExpectedEndDate { get; set; }
        public DateTime ActualEndDate { get; set; }
        public decimal TotalCost { get; set; }
        public Guid MotorcycleId { get; set; }
        public Guid DeliveryPersonId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ReturnCalculationResultDTO
    {
        public decimal TotalCost { get; set; }
        public decimal BaseCost { get; set; }
        public decimal AdditionalCost { get; set; }
    }
}