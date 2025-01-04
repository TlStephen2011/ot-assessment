using System.Text.Json.Serialization;

namespace OT.Assessment.Domain.Helpers;

public class PagedResponse<T> where T : class
{
    [JsonPropertyName("data")]
    public IEnumerable<T> Data { get; set; }
    [JsonPropertyName("page")]
    public int Page { get; set; }
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
    [JsonPropertyName("total")]
    public int Total { get; set; }
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

}