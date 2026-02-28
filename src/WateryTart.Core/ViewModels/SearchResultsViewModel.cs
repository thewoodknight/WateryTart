using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WateryTart.Core.ViewModels;

public partial class SearchResultsViewModel : ViewModelBase<SearchResultsViewModel>
{
    public ObservableCollection<IViewModelBase>? Results2 { get; set; }
    public void SetResults(IEnumerable<IViewModelBase> results)
    {
        Results2 = new ObservableCollection<IViewModelBase>(results);
    }
}