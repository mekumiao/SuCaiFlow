using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SuCaiFlow.Abstractions;

public class SuCaiFlowTaskRequest {
    [MaxLength(200)]
    public string? Name { get; set; }
    [MaxLength(50)]
    public string? SiteIdentifier { get; set; }
    [MaxLength(500)]
    public string? SearchKeywords { get; set; }
    public int AssetsToCollectCount { get; set; }
    [MaxLength(500)]
    public string? StartUrl { get; set; }
    public Dictionary<string, JsonElement>? Parameters { get; set; }
}
