using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class AlbumsListView : ReactiveUserControl<AlbumsListViewModel>
{
    public AlbumsListView()
    {
        InitializeComponent();
    }
}