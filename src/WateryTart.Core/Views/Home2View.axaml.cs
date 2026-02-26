using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class Home2View : ReactiveUserControl<Home2ViewModel>
{
    public Home2View()
    {
        InitializeComponent();
    }
}