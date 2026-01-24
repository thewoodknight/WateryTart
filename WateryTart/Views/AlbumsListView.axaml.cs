using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Avalonia;
using WateryTart.ViewModels;

namespace WateryTart.Views;

public partial class AlbumsListView : ReactiveUserControl<AlbumsListViewModel>
{
    public AlbumsListView()
    {
        InitializeComponent();
    }
}