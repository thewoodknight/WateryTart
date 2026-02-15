using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace WateryTart.Core.ViewModels;

public partial class SearchResultsViewModel : ReactiveObject, IViewModelBase
{
    private readonly IScreen _hostScreen;
    public IScreen HostScreen => _hostScreen;
    public string Icon { get; } = string.Empty;
    public ObservableCollection<IViewModelBase>? Results2 { get; set; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
    public string Title { get; set; } = string.Empty;
    public string? UrlPathSegment { get; } = string.Empty;
    [Reactive] public partial bool IsLoading { get; set; } = false;
    public SearchResultsViewModel(IScreen hostScreen)
    {
        _hostScreen = hostScreen;
    }

    public void SetResults(IEnumerable<IViewModelBase> results)
    {
        Results2 = new ObservableCollection<IViewModelBase>(results);
    }
}