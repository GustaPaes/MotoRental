using System.Text.Json.Serialization;

namespace MotoRental.API.DTOs
{
    public class RentalCreateDto
    {
        [JsonPropertyName("entregador_id")]
        public Guid DeliveryPersonId { get; set; }

        [JsonPropertyName("moto_id")]
        public Guid MotorcycleId { get; set; }

        [JsonPropertyName("data_inicio")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("data_termino")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("data_previsao_termino")]
        public DateTime ExpectedEndDate { get; set; }

        [JsonPropertyName("plano")]
        public int Plan { get; set; }
    }

    public class RentalResponseDto
    {
        [JsonPropertyName("identificador")]
        public Guid Id { get; set; }

        [JsonPropertyName("valor_diaria")]
        public decimal DailyCost { get; set; }

        [JsonPropertyName("entregador_id")]
        public Guid DeliveryPersonId { get; set; }

        [JsonPropertyName("moto_id")]
        public Guid MotorcycleId { get; set; }

        [JsonPropertyName("data_inicio")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("data_termino")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("data_previsao_termino")]
        public DateTime ExpectedEndDate { get; set; }

        [JsonPropertyName("data_devolucao")]
        public DateTime? ReturnDate { get; set; }
    }

    public class RentalUpdateDto
    {
        [JsonPropertyName("data_devolucao")]
        public DateTime ActualEndDate { get; set; }
    }
}