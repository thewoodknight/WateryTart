using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WateryTart.Service.MassClient.Models;

public abstract class MediaItemBase
{
    [JsonProperty("item_id")] public string? ItemId { get; set; }
    public string? Provider { get; set; }
    public string? Name { get; set; }
    public string? Version { get; set; }
    [JsonProperty("sort_name")] public string? SortName { get; set; }
    public string? Uri { get; set; }
    [JsonProperty("external_ids")] public List<List<string>>? ExternalIds { get; set; }
    [JsonProperty("is_playable")] public bool IsPlayable { get; set; }
    [JsonProperty("translation_key")] public object? TranslationKey { get; set; }

    [JsonProperty("media_type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public MediaType MediaType { get; set; }

    [JsonProperty("provider_mappings")] public List<ProviderMapping>? ProviderMappings { get; set; }
    public Metadata? Metadata { get; set; }
    public bool Favorite { get; set; }
    public int? Year { get; set; }

    public Image? image { get; set; }
}