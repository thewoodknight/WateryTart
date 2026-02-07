using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using WateryTart.Service.MassClient.Models.Enums;

namespace WateryTart.Service.MassClient.Models;

public partial class CurrentMedia : INotifyPropertyChanged
{
    [JsonPropertyName("uri")]
    public string? uri { get; set; }
    
    [JsonPropertyName("media_type")]
    public MediaType media_type { get; set; }
    
    [JsonPropertyName("title")]
    public string? title { get; set; }
    
    [JsonPropertyName("artist")]
    public string? artist { get; set; }
    
    [JsonPropertyName("album")]
    public string? album { get; set; }
    
    [JsonPropertyName("image_url")]
    public string? image_url { get; set; }
    
    [JsonPropertyName("duration")]
    public int? duration { get; set; }
    
    [JsonPropertyName("queue_id")]
    public string? queue_id { get; set; }
    
    [JsonPropertyName("queue_item_id")]
    public string? queue_item_id { get; set; }

    public double? elapsed_time
    {
        get => field;
        set
        {
            if (value.HasValue)
            {
                field = value.Value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("Progress");
            }
        }
    }

    public double progress
    {
        get
        {
            if (duration == null || duration == 0 || elapsed_time == null)
                return 0;
            
            return ((double)elapsed_time / (double)duration) * 100;
        }
    }

    public double? elapsed_time_last_updated { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}