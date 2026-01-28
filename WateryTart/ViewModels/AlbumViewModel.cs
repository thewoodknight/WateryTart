using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using WateryTart.MassClient;
using WateryTart.MassClient.Messages;
using WateryTart.MassClient.Models;
using WateryTart.MassClient.Responses;
using WateryTart.Services;
using WateryTart.ViewModels.Menus;

namespace WateryTart.ViewModels;

public partial class AlbumViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;
    private Album _album;
    public string? UrlPathSegment { get; } = "Album/ID";
    public IScreen HostScreen { get; }
    public bool ShowMiniPlayer { get => true; }
    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial Album Album { get; set; }

    public ObservableCollection<Item> Tracks { get; set; }
    public ReactiveCommand<Unit, Unit> PlayAlbumCommand { get; }
    public ReactiveCommand<Item, Unit> TrackTappedCommand { get; }
    public ReactiveCommand<Item, Unit> TrackAltMenuCommand { get; }

    public MenuViewModel MenuViewModel { get; set; }

    public AlbumViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;

        PlayAlbumCommand = ReactiveCommand.Create(() => _playersService.PlayItem(Album, mode: PlayMode.Replace));
        TrackTappedCommand = ReactiveCommand.Create<Item>((t) => _playersService.PlayItem(t, mode: PlayMode.Replace));

        TrackAltMenuCommand = ReactiveCommand.Create<Item>((t) =>
        {
            var menu = new MenuViewModel();
            menu.AddMenuItem(new MenuItemViewModel("Show Info", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {
                Debug.WriteLine("got here");
            })));
            menu.AddMenuItem(new MenuItemViewModel("Go To Artist", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {
                Debug.WriteLine("got here");
            })));
            menu.AddMenuItem(new MenuItemViewModel("Go To Album", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {
                Debug.WriteLine("got here");
            })));
            menu.AddMenuItem(new MenuItemViewModel("Remove from library", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {
                Debug.WriteLine("got here");
            })));
            menu.AddMenuItem(new MenuItemViewModel("Add to favourites", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {
                Debug.WriteLine("got here");
            })));
            menu.AddMenuItem(new MenuItemViewModel("Add to playlist", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {
                Debug.WriteLine("got here");
            })));
            menu.AddMenuItem(new MenuItemViewModel("Play", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {
                Debug.WriteLine("got here");
            })));

            foreach (var p in playersService.Players)
            {
                menu.AddMenuItem(new MenuItemViewModel($"\tPlay on {p.DisplayName}", string.Empty, ReactiveCommand.Create<Unit>(r =>
                {
                    _playersService.PlayItem(t, p);
                })));
            }

            MessageBus.Current.SendMessage(menu);
            //send message to display menu
        });
    }

    public void LoadFromId(string id, string provider)
    {
        Tracks = new ObservableCollection<Item>();

        _massClient.MusicAlbumTracks(id, provider, TrackListHandler);
        _massClient.MusicAlbumGet(id, provider, AlbumHandler);
    }

    public void Load(Album album)
    {
        Album = album;
        Tracks = new ObservableCollection<Item>();

        _massClient.MusicAlbumTracks(album.ItemId, TrackListHandler);
    }

    public void AlbumHandler(AlbumResponse response)
    {
        this.Album = response.Result;
        Title = Album.Name;
    }

    public void TrackListHandler(TracksResponse response)
    {
        foreach (var t in response.Result)
            Tracks.Add(t);
    }
}