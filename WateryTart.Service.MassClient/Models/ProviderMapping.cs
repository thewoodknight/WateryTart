using Newtonsoft.Json;

namespace WateryTart.Service.MassClient.Models;

public class ProviderMapping
{
    [JsonProperty("item_id")] public string? ItemId { get; set; }
    [JsonProperty("provider_domain")] public string? ProviderDomain { get; set; }
    [JsonProperty("provider_instance")] public string? ProviderInstance { get; set; }
    public bool? Available { get; set; }
    [JsonProperty("in_library")] public bool? InLibrary { get; set; }
    [JsonProperty("is_unique")] public object? IsUnique { get; set; }
    [JsonProperty("audio_format")] public AudioFormat? AudioFormat { get; set; }
    public string? Url { get; set; }
    public object? Details { get; set; }
}