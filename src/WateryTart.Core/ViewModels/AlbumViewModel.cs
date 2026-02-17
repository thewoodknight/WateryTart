using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;
using WateryTart.MusicAssistant.WsExtensions;
using WateryTart.Core.ViewModels.Popups;

namespace WateryTart.Core.ViewModels;

public partial class AlbumViewModel : ReactiveObject, IViewModelBase
{
    private readonly MusicAssistantClient _massClient;
    private readonly PlayersService _playersService;
    public string? UrlPathSegment { get; } = "Album/ID";
    public IScreen HostScreen { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
    [Reactive] public partial string Title { get; set; } = string.Empty;
    [Reactive] public partial Album? Album { get; set; }
    [Reactive] public partial bool IsLoading { get; set; }

    [Reactive] public ObservableCollection<TrackViewModel> Tracks { get; set; }
    public AsyncRelayCommand PlayAlbumCommand { get; }
    public AsyncRelayCommand<Item?> TrackTappedCommand { get; }
    public RelayCommand AlbumAltMenuCommand { get; }

    public RelayCommand AlbumFullViewCommand { get; }
    public AlbumViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService, Album? a = null)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Album = a;
        Tracks = new ObservableCollection<TrackViewModel>();

        PlayAlbumCommand = new AsyncRelayCommand(async () =>
        {
            if (Album != null)
                if (_playersService.SelectedPlayer == null)
                {
                    MessageBus.Current.SendMessage<IPopupViewModel>(MenuHelper.BuildStandardPopup(_playersService, Album));
                    await Task.CompletedTask;
                }
                else
                {
                    await _playersService.PlayItem(Album, mode: PlayMode.Replace);
                }
        });

        TrackTappedCommand = new AsyncRelayCommand<Item?>((t) =>
        {
            if (t != null) 
                return _playersService.PlayItem(t, mode: PlayMode.Replace);
            return null!;
        });
        AlbumFullViewCommand = new RelayCommand(() =>
        {
            if (Album.ItemId != null && Album.Provider != null)
                LoadFromId(Album.ItemId, Album.Provider);
            screen.Router.Navigate.Execute(this);
        });
        AlbumAltMenuCommand = new RelayCommand(() =>
        {
            MessageBus.Current.SendMessage<IPopupViewModel>(MenuHelper.BuildStandardPopup(playersService, Album));
        });
    }

    public void LoadFromId(string id, string provider)
    {
#pragma warning disable CS4014 // Fire-and-forget intentional - loads data asynchronously
        _ = LoadAlbumDataAsync(id, provider);
#pragma warning restore CS4014
    }

    public void Load(Album album)
    {
        Album = album;
#pragma warning disable CS4014 // Fire-and-forget intentional - loads data asynchronously
        if (album.ItemId != null && album.Provider != null)
            _ = LoadAlbumDataAsync(album.ItemId, album.Provider);
#pragma warning restore CS4014
    }

    private async Task LoadAlbumDataAsync(string id, string provider)
    {
        try
        {
            var albumResponse = await _massClient.WithWs().GetMusicAlbumAsync(id, provider);
            Album = albumResponse.Result;
            if (Album != null && Album.Name != null)
                Title = Album.Name;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading album: {ex.Message}");
        }

        try
        {
            var tracksResponse = await _massClient.WithWs().GetMusicAlbumTracksAsync(id, provider);
            if (tracksResponse.Result != null)
                foreach (var t in tracksResponse.Result)
                    Tracks.Add(new TrackViewModel(_massClient, HostScreen, _playersService, t));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading tracks: {ex.Message}");
        }
    }
}
