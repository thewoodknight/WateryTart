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
            menu.AddMenuItem(new Menus.MenuItemViewModel("Show Info", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {

            })));
            menu.AddMenuItem(new Menus.MenuItemViewModel("Go To Artist", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {

            })));
            menu.AddMenuItem(new Menus.MenuItemViewModel("Go To Album", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {

            })));
            menu.AddMenuItem(new Menus.MenuItemViewModel("Remove from library", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {

            })));
            menu.AddMenuItem(new Menus.MenuItemViewModel("Add to favourites", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {

            })));
            menu.AddMenuItem(new Menus.MenuItemViewModel("Add to playlist", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {

            })));
            menu.AddMenuItem(new Menus.MenuItemViewModel("Play", string.Empty, ReactiveCommand.Create<Unit>(r =>
            {

            })));

            foreach (var p in playersService.Players)
            {
                menu.AddMenuItem(new Menus.MenuItemViewModel($"\tPlay on {p.DisplayName}", string.Empty, ReactiveCommand.Create<Unit>(r =>
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
        _ = LoadAlbumDataAsync(id, provider);
    }

    public void Load(Album album)
    {
        Album = album;
        Tracks = new ObservableCollection<Item>();
        _ = LoadTracksAsync(album.ItemId, "library");
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
                Tracks.Add(t);
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
                Tracks.Add(t);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading tracks: {ex.Message}");
        }
    }
}
