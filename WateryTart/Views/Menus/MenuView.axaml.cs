using ReactiveUI.Avalonia;
using WateryTart.ViewModels.Menus;

namespace WateryTart.Views.Menus;

public partial class MenuView : ReactiveUserControl<MenuViewModel>
{
    public MenuView()
    {
        InitializeComponent();
    }
}