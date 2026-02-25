using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WateryTart.Core.Settings;
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

    public RelayCommand<IViewModelBase> GoToSettings { get; set; }
    public SettingsViewModel(IEnumerable<IHaveSettings> settingsProviders, IScreen screen)
    {
        _screen = screen;
        SettingsProviders = new ObservableCollection<IHaveSettings>(settingsProviders);

        GoToSettings = new RelayCommand<IViewModelBase>(item =>
        {
                screen.Router.Navigate.Execute(item);
        });
    }
}