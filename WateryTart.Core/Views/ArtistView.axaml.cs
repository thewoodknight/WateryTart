using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class ArtistView : ReactiveUserControl<ArtistViewModel>
{
    public ArtistView()
    {
        InitializeComponent();
    }
}