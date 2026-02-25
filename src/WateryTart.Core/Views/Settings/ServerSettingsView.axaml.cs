using Avalonia.Controls;
using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class ServerSettingsView : ReactiveUserControl<ServerSettingsViewModel>
{
    public ServerSettingsView()
    {
        InitializeComponent();
    }
}
