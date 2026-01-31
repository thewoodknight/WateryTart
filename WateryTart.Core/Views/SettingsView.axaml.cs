using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
    }
}