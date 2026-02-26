using ReactiveUI.Avalonia;
using WateryTart.Platform.Windows.ViewModels;

namespace WateryTart.Platform.Windows.Views;

public partial class PlaybackSettingsView : ReactiveUserControl<PlaybackSettingsViewModel>
{
    public PlaybackSettingsView()
    {
        InitializeComponent();
    }
}
