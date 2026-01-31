using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class PlayersView : ReactiveUserControl<PlayersViewModel>
{
    public PlayersView()
    {
        InitializeComponent();
    }
}