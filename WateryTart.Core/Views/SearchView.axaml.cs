using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class SearchView : ReactiveUserControl<SearchViewModel>
{
    public SearchView()
    {
        InitializeComponent();
    }
}