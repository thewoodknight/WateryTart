using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WateryTart.Core.Extensions;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.ViewModels;

public partial class SimilarTracksViewModel : ReactiveObject, IViewModelBase
{
    private readonly MusicAssistantClient _client;
    public string? UrlPathSegment { get; } = "SimilarTracks/ID";
    public IScreen HostScreen { get; }
    public string Title { get; } = string.Empty;
    public bool ShowMiniPlayer { get; } = true;
    public bool ShowNavigation { get; } = true;
    [Reactive] public partial bool IsLoading { get; set; }
    public ObservableCollection<IViewModelBase> Tracks { get; set; } = new ObservableCollection<IViewModelBase>();
    public SimilarTracksViewModel(IScreen screen, MusicAssistantClient client)
    {
        _client = client;
        HostScreen = screen;
    }

    public async Task LoadFromId(string id, string provider)
    {
        var results = await _client.WithWs().GetMusicSimilarTracksAsync(id, provider);
        foreach (var i in results.Result)
        {
           Tracks.Add(i.CreateViewModelForItem());
        }
    }

}
