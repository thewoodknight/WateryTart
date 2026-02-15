using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
namespace WateryTart.Core.ViewModels;

public partial class SettingsViewModel : ReactiveObject, IViewModelBase
{
    [Reactive] public partial bool IsLoading { get; set; }
    private readonly IScreen _screen;
    public ObservableCollection<IHaveSettings> SettingsProviders { get; set; }
    public string? UrlPathSegment { get; } = "settings";
    public IScreen HostScreen => _screen;
    public bool ShowMiniPlayer => false;
    public string Title => "Settings";

    public bool ShowNavigation => true;

    public SettingsViewModel(IEnumerable<IHaveSettings> settingsProviders, IScreen screen)
    {
        _screen = screen;
        SettingsProviders = new ObservableCollection<IHaveSettings>(settingsProviders);
    }
}