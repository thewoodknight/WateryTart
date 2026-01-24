using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using WateryTart.MassClient;
using WateryTart.MassClient.Models;
using WateryTart.MassClient.Responses;
using WateryTart.Services;

namespace WateryTart.ViewModels;

public class AlbumViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly IMassWSClient _massClient;
    private readonly IPlayersService _playersService;
    public string? UrlPathSegment { get; } = "Album/ID";
    public IScreen HostScreen { get; }
    public Album Album { get; set; }

    public ObservableCollection<Track> Tracks { get; set; }
    public AlbumViewModel(IMassWSClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        
    }

    public void Load(Album album)
    {
        Album = album;
        Tracks = new ObservableCollection<Track>();

        _massClient.MusicAlbumGet(album.item_id, TrackListHandler);
    }

    public void TrackListHandler(TracksResponse response)
    {
        foreach (var t in response.result)
            Tracks.Add(t);


        var x = Tracks.Last();
        _playersService.Play(x);
    }
}
