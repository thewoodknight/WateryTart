namespace WateryTart.MassClient.Models;

public class PlayerQueue
{
    public string queue_id { get; set; }
    public bool active { get; set; }
    public string display_name { get; set; }
    public bool available { get; set; }
    public Int64 items { get; set; }
    public bool shuffle_enabled { get; set; }
    public string repeat_mode { get; set; }
    public bool dont_stop_the_music_enabled { get; set; }
    public Int64? current_index { get; set; }
    public Int64? index_in_buffer { get; set; }
    public double? elapsed_time { get; set; }
    public double? elapsed_time_last_updated { get; set; }
    public string state { get; set; }
    public QueuedItem current_item { get; set; }
    public QueuedItem next_item { get; set; }
    public List<object> radio_source { get; set; }
    public bool flow_mode { get; set; }
    public Int64 resume_pos { get; set; }
}