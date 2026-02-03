using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WateryTart.Service.MassClient.Models;

public class PlayerQueue : INotifyPropertyChanged
{
    private long? current_index1;

    public string? queue_id { get; set; }
    public bool active { get; set; }
    public string? display_name { get; set; }
    public bool available { get; set; }
    public Int64 items { get; set; }
    public bool shuffle_enabled { get; set; }
    public string? repeat_mode { get; set; }
    public bool dont_stop_the_music_enabled { get; set; }
    public Int64? current_index { get => current_index1;
        set
        {
            current_index1 = value; NotifyPropertyChanged();

        } }
    public Int64? index_in_buffer { get; set; }
    public double? elapsed_time { get; set; }
    public double? elapsed_time_last_updated { get; set; }
    public string? state { get; set; }

    public QueuedItem? current_item
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
        }
    }

    public QueuedItem? next_item { get; set; }
    public List<object>? radio_source { get; set; }
    public bool flow_mode { get; set; }
    public Int64 resume_pos { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}