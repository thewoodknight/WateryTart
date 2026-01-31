using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class ArtistsView : ReactiveUserControl<ArtistsViewModel>
{
    public ArtistsView()
    {
        InitializeComponent();
    }
}