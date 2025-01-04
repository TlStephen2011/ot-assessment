using System.Text.Json.Serialization;

namespace OT.Assessment.Domain.Models.Dtos;

public class TopSpendersResponseDto
{
    [JsonPropertyName("accountId")]
    public Guid AccountId { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("totalAmountSpend")]
    public double TotalAmountSpent { get; set; }
}