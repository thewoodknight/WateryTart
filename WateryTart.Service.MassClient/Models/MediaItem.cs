using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WateryTart.Service.MassClient.Models;

public class MediaItem : MediaItemBase, INotifyPropertyChanged
{
    public object? position { get; set; }
    public int duration { get; set; }
    public List<Artist>? artists { get; set; }
    public int last_played { get; set; }
    public Album? album { get; set; }
    public int disc_number { get; set; }
    public int track_number { get; set; }


    public double elapsed_time
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
        }
    } = 0;
    public double progress => (elapsed_time / duration) * 100;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}