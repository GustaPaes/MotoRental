using MotoRental.Domain.Enums;

namespace MotoRental.Domain.Entities
{
    public class DeliveryPerson
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string CnhNumber { get; set; } = string.Empty;
        public CnhType CnhType { get; set; }
        public string CnhImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
