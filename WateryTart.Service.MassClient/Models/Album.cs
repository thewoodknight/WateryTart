using Newtonsoft.Json;

namespace WateryTart.Service.MassClient.Models;

public class Album : MediaItemBase
{
    public List<Artist>? Artists { get; set; }
    [JsonProperty("album_type")] public string? AlbumType { get; set; }
}