using System.Text.Json.Serialization;

namespace DesignerCloset.Models

{
    public class OrderFunction
    {
        [JsonPropertyName("CustomerName")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("ProductName")]
        public string? ProductName { get; set; }

        [JsonPropertyName("Total")]
        public double? Total { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }

    }
}

