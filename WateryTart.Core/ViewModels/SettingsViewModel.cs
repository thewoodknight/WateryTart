using ReactiveUI;

namespace WateryTart.Core.ViewModels;

public class SettingsViewModel : ReactiveObject, IViewModelBase
{
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public bool ShowMiniPlayer { get => false; }
    public string Title
    {
        get => "Settings";
        set;
    }

    public bool ShowNavigation => true;
}