using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class GeneralSettingsView : ReactiveUserControl<GeneralSettingsViewModel>
{
    public GeneralSettingsView()
    {
        InitializeComponent();
    }
}