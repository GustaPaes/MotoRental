namespace MotoRental.Application.DTOs
{
    public class DeliveryPersonCreateDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string CnhNumber { get; set; } = string.Empty;
        public string CnhType { get; set; } = string.Empty;
    }

    public class DeliveryPersonResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string CnhNumber { get; set; } = string.Empty;
        public string CnhType { get; set; } = string.Empty;
        public string CnhImageUrl { get; set; } = string.Empty;
    }
}