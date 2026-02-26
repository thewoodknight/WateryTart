using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.ViewModels;

public abstract class ViewModelBase<T> : ReactiveObject, IViewModelBase
{
    internal ILogger<T> _logger;
    internal PlayersService? _playersService;
    internal ISettings? _settings;
    internal MusicAssistantClient _client;
    public IScreen HostScreen { get; set; }
    public virtual bool IsLoading { get; set; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
    public virtual string Title { get; }
    public string UrlPathSegment => string.Empty;

#pragma warning disable CS8618
    public ViewModelBase(
        ILoggerFactory loggerFactory, 
        MusicAssistantClient? client = null)
#pragma warning restore CS8618 
    {
        _logger = CreateLogger(loggerFactory);
        if (client != null)
        _client = client;
    }

    internal ILogger<T> CreateLogger(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger<T>();
    }
}

public partial class Home2ViewModel : ViewModelBase<Home2ViewModel>
{
    public override string Title => "Home";
    [Reactive] public override partial bool IsLoading { get; set; }
    [Reactive] public partial ObservableCollection<TrackViewModel> RecentTracks { get; set; }

    [Reactive] public partial ObservableCollection<AlbumViewModel> DiscoverAlbums { get; set; }

    [Reactive] public partial ObservableCollection<ArtistViewModel> DiscoverArtists { get; set; }
    public Home2ViewModel(
        IScreen screen,
        MusicAssistantClient maClient,
        ISettings settings,
        PlayersService playersService,
        ILoggerFactory loggerFactory) : base(loggerFactory, maClient)
    {
        _settings = settings;
        _playersService = playersService;
        HostScreen = screen;

        DiscoverArtists = new ObservableCollection<ArtistViewModel>();
        DiscoverAlbums = new ObservableCollection<AlbumViewModel>();
        RecentTracks = new ObservableCollection<TrackViewModel>();

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            var tasks = new List<Task>
            {
                Task.Run(async () =>
                {
                    //Recent Tracks - api call
                    _logger.LogInformation("Fetching recent tracks...");
                    var recent = await _client.WithWs().GetRecentlyPlayedItemsAsync(limit: 10);
                    foreach (var r in recent.Result!)
                    {
                        var track = App.Container.GetRequiredService<TrackViewModel>();
                        track.Track = r;
                        RecentTracks.Add(track);
                    }

                }),

                Task.Run(async () =>
                {
                    //Discover Albums - 
                    var albums = await _client.WithWs().GetMusicAlbumsLibraryItemsAsync(limit: 10, order_by: "random");

                    foreach (var a in albums.Result!)
                    {
                        var album = App.Container.GetRequiredService<AlbumViewModel>();
                        album.Album = a;

                        DiscoverAlbums.Add(album);
                    }

                    _logger.LogInformation("Fetching discover albums...");

                }),

                Task.Run(async () =>
                {
                    //Discover Artists
                    var artists = await _client.WithWs().GetArtistsAsync(limit: 10, order_by: "random", album_artists_only: true);

                    foreach (var a in artists.Result!)
                    {
                        var artist = App.Container.GetRequiredService<ArtistViewModel>();
                        artist.Artist = a;

                        DiscoverArtists.Add(artist);
                    }
                    _logger.LogInformation("Fetching discover artists...");

                })
            };

            //run all tasks simultaneously and wait for them to complete
            await System.Threading.Tasks.Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Home2ViewModel");
        }
        finally
        {
            IsLoading = false;
        }
    }
}