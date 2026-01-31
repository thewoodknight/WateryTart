using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    public HomeView()
    {
        InitializeComponent();
    }
}