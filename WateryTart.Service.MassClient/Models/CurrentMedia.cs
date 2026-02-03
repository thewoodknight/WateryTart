using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WateryTart.Service.MassClient.Models;

public partial class CurrentMedia : INotifyPropertyChanged
{
    private double elapsed_time1;

    public string? uri { get; set; }
    public string? media_type { get; set; }
    public string? title { get; set; }
    public string? artist { get; set; }
    public string? album { get; set; }
    public string? image_url { get; set; }
    public double? duration { get; set; }
    public string? source_id { get; set; }
    public string? queue_item_id { get; set; }
    public object? custom_data { get; set; }
    public double? elapsed_time
    {
        get => elapsed_time1;
        set
        {
            if (value.HasValue)
            {
                elapsed_time1 = value.Value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("progress");
            }
        }
    }

    public double progress => (elapsed_time.Value / duration.Value) * 100;

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