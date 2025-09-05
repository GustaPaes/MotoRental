using System.Text.Json.Serialization;

namespace MotoRental.API.DTOs
{
    public class DeliveryPeopleCreateDto
    {
        [JsonPropertyName("nome")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("cnpj")]
        public string Cnpj { get; set; } = string.Empty;

        [JsonPropertyName("data_nascimento")]
        public DateTime BirthDate { get; set; }

        [JsonPropertyName("numero_cnh")]
        public string CnhNumber { get; set; } = string.Empty;

        [JsonPropertyName("tipo_cnh")]
        public string CnhType { get; set; } = string.Empty;

        [JsonPropertyName("imagem_cnh")]
        public string ImagemCnh { get; set; } = string.Empty;
    }

    public class CnhImageUpdateDto
    {
        [JsonPropertyName("imagem_cnh")]
        public string ImagemCnh { get; set; } = string.Empty;
    }
}