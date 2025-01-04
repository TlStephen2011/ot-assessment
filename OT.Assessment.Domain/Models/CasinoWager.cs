using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OT.Assessment.Domain.Models;

public class CasinoWager
{
    [Required]
    [JsonPropertyName("wagerId")]
    public string WagerId { get; set; }

    [Required]
    [JsonPropertyName("theme")]
    public string Theme { get; set; }

    [Required]
    [JsonPropertyName("provider")]
    public string Provider { get; set; }

    [Required]
    [JsonPropertyName("gameName")]
    public string GameName { get; set; }

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; }

    [Required]
    [JsonPropertyName("brandId")]
    public string BrandId { get; set; }

    [Required]
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; }

    [JsonPropertyName("Username")]
    public string Username { get; set; }

    [JsonPropertyName("externalReferenceId")]
    public string ExternalReferenceId { get; set; }

    [JsonPropertyName("transactionTypeId")]
    public string TransactionTypeId { get; set; }

    [Required]
    [JsonPropertyName("amount")]
    public double Amount { get; set; }

    [JsonPropertyName("createdDateTime")]
    public DateTime CreatedDateTime { get; set; }

    [Required]
    [JsonPropertyName("numberOfBets")]
    public int NumberOfBets { get; set; }

    [Required]
    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; }

    [JsonPropertyName("sessionData")]
    public string SessionData { get; set; }

    [JsonPropertyName("Duration")]
    public long Duration { get; set; }
}