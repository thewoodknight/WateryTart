using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WateryTart.Core.Settings;
namespace WateryTart.Core.ViewModels;

public partial class SettingsViewModel : ViewModelBase<SettingsViewModel>
{   public ObservableCollection<IHaveSettings> SettingsProviders { get; set; }
    public override string Title => "Settings";
    public RelayCommand<IViewModelBase> GoToSettings { get; set; }
    public SettingsViewModel(IEnumerable<IHaveSettings> settingsProviders, IScreen screen, ILoggerFactory loggerFactory)
    {
        HostScreen = screen;
        SettingsProviders = new ObservableCollection<IHaveSettings>(settingsProviders);

        GoToSettings = new RelayCommand<IViewModelBase>(item =>
        {
                screen.Router.Navigate.Execute(item);
        });
    }
}