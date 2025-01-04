using System.Text.Json.Serialization;

namespace OT.Assessment.Domain.Models.Dtos;

public class CasinoWagersResponseDto
{
    [JsonPropertyName("wagerId")]
    public Guid WagerId { get; set; }
    [JsonPropertyName("game")]
    public string Game { get; set; }
    [JsonPropertyName("provider")]
    public string Provider { get; set; }
    [JsonPropertyName("amount")]
    public double Amount { get; set; }
    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; }
}