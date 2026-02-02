using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public partial class SearchResultsViewModel : ReactiveObject, IViewModelBase
{
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; set; }
    public bool ShowMiniPlayer { get; }
    public bool ShowNavigation { get; }

    public ObservableCollection<IViewModelBase> Results2 { get; set; }
    
    public void SetResults(IEnumerable<IViewModelBase> results)
    {
        Results2 = new ObservableCollection<IViewModelBase>(results);
    }
}