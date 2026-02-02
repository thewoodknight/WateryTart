using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class SearchResultsView : ReactiveUserControl<SearchResultsViewModel>
{
    public SearchResultsView()
    {
        InitializeComponent();
    }
}