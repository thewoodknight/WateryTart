using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public partial class AlbumViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;
    private Album _album;
    public string? UrlPathSegment { get; } = "Album/ID";
    public IScreen HostScreen { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial Album Album { get; set; }

    public ObservableCollection<TrackViewModel> Tracks { get; set; }
    public ReactiveCommand<Unit, Unit> PlayAlbumCommand { get; }
    public ReactiveCommand<Item, Unit> TrackTappedCommand { get; }
    public ReactiveCommand<Unit, Unit> AlbumAltMenuCommand { get; }

    public ReactiveCommand<Unit, Unit> AlbumFullViewCommand { get; }
    public AlbumViewModel()
    {

    }
    public AlbumViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService, Album a = null)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Album = a;

        PlayAlbumCommand = ReactiveCommand.CreateFromTask(() => _playersService.PlayItem(Album, mode: PlayMode.Replace));
        TrackTappedCommand = ReactiveCommand.CreateFromTask<Item>((t) => _playersService.PlayItem(t, mode: PlayMode.Replace));
        AlbumFullViewCommand = ReactiveCommand.Create(() =>
        {
            this.LoadFromId(Album.ItemId, Album.Provider);
            screen.Router.Navigate.Execute(this);
        });
        AlbumAltMenuCommand = ReactiveCommand.Create(() =>
        {
            MessageBus.Current.SendMessage(MenuHelper.BuildStandardPopup(playersService, Album));
        });
    }

    public void LoadFromId(string id, string provider)
    {
        Tracks = new ObservableCollection<TrackViewModel>();
#pragma warning disable CS4014 // Fire-and-forget intentional - loads data asynchronously
        _ = LoadAlbumDataAsync(id, provider);
#pragma warning restore CS4014
    }

    public void Load(Album album)
    {
        Album = album;
        Tracks = new ObservableCollection<TrackViewModel>();
#pragma warning disable CS4014 // Fire-and-forget intentional - loads data asynchronously
        _ = LoadAlbumDataAsync(album.ItemId, album.Provider);
#pragma warning restore CS4014
    }

    private async Task LoadAlbumDataAsync(string id, string provider)
    {
        try
        {
            var albumResponse = await _massClient.MusicAlbumGetAsync(id, provider);
            Album = albumResponse.Result;
            Title = Album.Name;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading album: {ex.Message}");
        }

        try
        {
            var tracksResponse = await _massClient.MusicAlbumTracksAsync(id, provider);
            foreach (var t in tracksResponse.Result)
                Tracks.Add(new TrackViewModel(_massClient, HostScreen, _playersService, t));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading tracks: {ex.Message}");
        }
    }

    private async Task LoadTracksAsync(string id, string provider)
    {
        try
        {
            var tracksResponse = await _massClient.MusicAlbumTracksAsync(id, provider);
            foreach (var t in tracksResponse.Result)
                Tracks.Add(new TrackViewModel(_massClient, HostScreen, _playersService, t));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading tracks: {ex.Message}");
        }
    }
}
