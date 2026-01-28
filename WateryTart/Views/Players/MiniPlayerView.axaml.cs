using ReactiveUI.Avalonia;
using WateryTart.ViewModels.Players;

namespace WateryTart.Views.Players;

public partial class MiniPlayerView : ReactiveUserControl<MiniPlayerViewModel>
{
    public MiniPlayerView()
    {
        InitializeComponent();
    }
}