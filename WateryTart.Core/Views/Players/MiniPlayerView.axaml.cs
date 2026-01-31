using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels.Players;

namespace WateryTart.Core.Views.Players;

public partial class MiniPlayerView : ReactiveUserControl<MiniPlayerViewModel>
{
    public MiniPlayerView()
    {
        InitializeComponent();
    }
}