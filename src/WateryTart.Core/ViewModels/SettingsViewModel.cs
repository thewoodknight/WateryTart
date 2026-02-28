using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WateryTart.Core.Settings;

namespace WateryTart.Core.ViewModels;

public partial class SettingsViewModel : ViewModelBase<SettingsViewModel>
{
    public RelayCommand<IViewModelBase> GoToSettings { get; set; }
    public ObservableCollection<IHaveSettings> SettingsProviders { get; set; }
    public override string Title => "Settings";

    public SettingsViewModel(IEnumerable<IHaveSettings> settingsProviders, IScreen screen)
    {
        HostScreen = screen;
        SettingsProviders = new ObservableCollection<IHaveSettings>(settingsProviders);

        GoToSettings = new RelayCommand<IViewModelBase>(item =>
        {
            HostScreen.Router.Navigate.Execute(item);
        });
    }
}