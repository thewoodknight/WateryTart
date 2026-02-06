using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels.Menus;

namespace WateryTart.Core.Views.Menus;

public partial class MenuView : ReactiveUserControl<MenuViewModel>
{
    public MenuView()
    {
        InitializeComponent();
    }
}