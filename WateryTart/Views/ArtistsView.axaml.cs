using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using WateryTart.ViewModels;

namespace WateryTart.Views;

public partial class ArtistsView : ReactiveUserControl<ArtistsViewModel>
{
    public ArtistsView()
    {
        InitializeComponent();
    }
}