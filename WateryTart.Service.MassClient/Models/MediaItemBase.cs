using System.Text.Json.Serialization;
using WateryTart.Service.MassClient.Models.Enums;

namespace WateryTart.Service.MassClient.Models;

public static partial class MediaItemBaseExtensions
{
    extension(MediaItemBase i)
    {
        public string? GetProviderInstance()
        {
            string? provider = string.Empty;
            if (!string.IsNullOrEmpty(i.Provider))
                provider = i.Provider;
            else if (i.ProviderMappings != null)
                provider = i.ProviderMappings.FirstOrDefault()?.ProviderInstance;

            return provider;
        }
    }
}


public abstract class MediaItemBase
{
    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    public string? Provider { get; set; }
    public string? Name { get; set; }
    [JsonPropertyName("version")] public string? Version { get; set; }

    [JsonPropertyName("sort_name")]
    public string? SortName { get; set; }

    public string? Uri { get; set; }

    [JsonPropertyName("external_ids")]
    public List<List<string>>? ExternalIds { get; set; }

    [JsonPropertyName("is_playable")]
    public bool IsPlayable { get; set; }

    [JsonPropertyName("translation_key")]
    public object? TranslationKey { get; set; }

    [JsonPropertyName("media_type")]
    public MediaType MediaType { get; set; }

    [JsonPropertyName("provider_mappings")]
    public List<ProviderMapping>? ProviderMappings { get; set; }
    [JsonPropertyName("metadata")] public Metadata? Metadata { get; set; }
    public bool Favorite { get; set; }
    [JsonPropertyName("year")] public int? Year { get; set; }
    public Image? image { get; set; }
}