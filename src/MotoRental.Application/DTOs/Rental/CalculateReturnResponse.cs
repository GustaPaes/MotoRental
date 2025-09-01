namespace MotoRental.Application.DTOs.Rental
{
    public class CalculateReturnResponse
    {
        public decimal TotalCost { get; set; }
        public string CostBreakdown { get; set; } = string.Empty;
    }
}