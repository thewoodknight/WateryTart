using System.Text.Json.Serialization;

namespace WateryTart.Service.MassClient.Models;

public class Recommendation : MediaItemBase
{
    [JsonPropertyName("path")] public string? Path { get; set; }
    [JsonPropertyName("icon")] public string? Icon { get; set; }
    [JsonPropertyName("items")] public List<Item>? Items { get; set; }
    [JsonPropertyName("subtitle")] public string? Subtitle { get; set; }
}