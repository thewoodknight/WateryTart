using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;


public partial class PlaylistView : ReactiveUserControl<PlaylistViewModel>
{
    public PlaylistView()
    {
        InitializeComponent();
    }
}