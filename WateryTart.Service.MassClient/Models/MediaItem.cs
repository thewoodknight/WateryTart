using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WateryTart.Service.MassClient.Models;

public class MediaItem : Item, INotifyPropertyChanged
{
    public double elapsed_time
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
        }
    } = 0;
    public double Progress
    {
        get
        {
            if (duration == null || duration == 0)
                    return 0;
            return (elapsed_time / duration.Value) * 100;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}