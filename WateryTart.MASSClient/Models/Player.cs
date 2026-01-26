using Newtonsoft.Json;

namespace WateryTart.MassClient.Models;

public class Player : ResultBase
{
    [JsonProperty("player_id")] public string PlayerId { get; set; }
    public string Provider { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public bool Available { get; set; }
    [JsonProperty("device_info")] public DeviceInfo DeviceInfo { get; set; }
    [JsonProperty("supported_features")] public List<string> SupportedFeatures { get; set; }
    [JsonProperty("playback_state")] public PlaybackState PlaybackState { get; set; }
    [JsonProperty("elapsed_time")] public double? ElapsedTime { get; set; }
    [JsonProperty("elapsed_time_last_updated")] public double? ElapsedTimeLastUpdated { get; set; }
    public bool Powered { get; set; }
    [JsonProperty("volume_level")] public int VolumeLevel { get; set; }
    [JsonProperty("volume_muted")] public bool? VolumeMuted { get; set; }
    [JsonProperty("group_members")] public List<object> GroupMembers { get; set; }
    [JsonProperty("static_group_members")] public List<object> StaticGroupMembers { get; set; }
    [JsonProperty("can_group_with")] public List<string> CanGroupWith { get; set; }
    [JsonProperty("synced_to")] public object SyncedTo { get; set; }
    [JsonProperty("active_source")] public string ActiveSource { get; set; }
    [JsonProperty("source_list")] public List<SourceList> SourceList { get; set; }
    [JsonProperty("active_group")] public object ActiveGroup { get; set; }
    [JsonProperty("current_media")] public CurrentMedia CurrentMedia { get; set; }
    public bool Enabled { get; set; }
    [JsonProperty("hide_player_in_ui")] public List<string> HidePlayerInUI { get; set; }
    [JsonProperty("expose_to_ha")] public bool ExposedToHA { get; set; }
    public string Icon { get; set; }
    [JsonProperty("group_volume")] public int GroupVolume { get; set; }
    [JsonProperty("extra_attributes")] public ExtraAttributes ExtraAttributes { get; set; }
    [JsonProperty("power_control")] public string PowerControl { get; set; }
    [JsonProperty("volume_control")] public string VolumeControl { get; set; }
    [JsonProperty("mute_control")] public string MuteControl { get; set; }

    [JsonProperty("display_name")] public string DisplayName { get; set; }
    public string state { get; set; }
    [JsonProperty("group_childs")] public List<object> GroupChilds { get; set; }
    [JsonProperty("extra_data")] public ExtraData ExtraData { get; set; }
}