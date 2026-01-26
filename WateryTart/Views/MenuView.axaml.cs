using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using WateryTart.ViewModels;

namespace WateryTart.Views;

public partial class MenuView : ReactiveUserControl<MenuViewModel>
{
    public MenuView()
    {
        InitializeComponent();
    }
}