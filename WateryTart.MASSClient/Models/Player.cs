namespace WateryTart.MassClient.Models;

public class Player : ResultBase
{
    public string player_id { get; set; }
    public string provider { get; set; }
    public string type { get; set; }
    public string name { get; set; }
    public bool available { get; set; }
    public DeviceInfo device_info { get; set; }
    public List<string> supported_features { get; set; }
    public string playback_state { get; set; }
    public double? elapsed_time { get; set; }
    public double? elapsed_time_last_updated { get; set; }
    public bool powered { get; set; }
    public int volume_level { get; set; }
    public bool? volume_muted { get; set; }
    public List<object> group_members { get; set; }
    public List<object> static_group_members { get; set; }
    public List<string> can_group_with { get; set; }
    public object synced_to { get; set; }
    public string active_source { get; set; }
    public List<SourceList> source_list { get; set; }
    public object active_group { get; set; }
    public CurrentMedia current_media { get; set; }
    public bool enabled { get; set; }
    public List<string> hide_player_in_ui { get; set; }
    public bool expose_to_ha { get; set; }
    public string icon { get; set; }
    public int group_volume { get; set; }
    public ExtraAttributes extra_attributes { get; set; }
    public string power_control { get; set; }
    public string volume_control { get; set; }
    public string mute_control { get; set; }
    public string display_name { get; set; }
    public string state { get; set; }
    public List<object> group_childs { get; set; }
    public ExtraData extra_data { get; set; }
}