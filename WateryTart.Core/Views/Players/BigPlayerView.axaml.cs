using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels.Players;

namespace WateryTart.Core.Views.Players;

public partial class BigPlayerView : ReactiveUserControl<BigPlayerViewModel>
{
    public BigPlayerView()
    {
        InitializeComponent();
    }
}