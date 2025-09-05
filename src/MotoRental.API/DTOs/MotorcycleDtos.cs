using System.Text.Json.Serialization;

namespace MotoRental.API.DTOs
{
    public class MotorcycleCreateRequest
    {
        [JsonPropertyName("ano")]
        public int Year { get; set; }

        [JsonPropertyName("modelo")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("placa")]
        public string LicensePlate { get; set; } = string.Empty;
    }

    public class MotorcycleResponse
    {
        [JsonPropertyName("identificador")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("ano")]
        public int Year { get; set; }

        [JsonPropertyName("modelo")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("placa")]
        public string LicensePlate { get; set; } = string.Empty;
    }

    public class LicensePlateUpdateRequest
    {
        [JsonPropertyName("placa")]
        public string LicensePlate { get; set; } = string.Empty;
    }
}