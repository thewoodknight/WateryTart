using Autofac;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.ViewModels;
public partial class Home2ViewModel : ViewModelBase<Home2ViewModel>
{
    public override string Title => "Home";
    [Reactive] public override partial bool IsLoading { get; set; }
    [Reactive] public partial ObservableCollection<TrackViewModel> RecentTracks { get; set; }
    [Reactive] public partial ObservableCollection<AlbumViewModel> DiscoverAlbums { get; set; }
    [Reactive] public partial ObservableCollection<ArtistViewModel> DiscoverArtists { get; set; }

    public ICommand DiscoverArtistsCommand { get; set; }
    public ICommand DiscoverAlbumsCommand { get; set; }
    public ICommand RecentlyPlayedTracksCommand { get; set; }
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

        DiscoverArtists = [];
        DiscoverAlbums = [];
        RecentTracks = [];

        //Set all commands
        DiscoverArtistsCommand = new RelayCommand(() => { });
        DiscoverAlbumsCommand = new RelayCommand(() => { });
        RecentlyPlayedTracksCommand = new RelayCommand(() => { });

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
                    if (RecentTracks.Count > 0) //Solves weird condition where this randomly gets hit twice.
                        return;
                    //Recent Tracks - api call
                    _logger.LogInformation("Fetching recent tracks...");
                    var recent = await _client.WithWs().GetRecentlyPlayedItemsAsync(limit: 5);
                    foreach (var r in recent.Result!)
                    {
                        if (RecentTracks.Any(t => t.Track.ItemId == r.ItemId)) // make sure no duplicates
                            continue;
                        var track = App.Container.Resolve<TrackViewModel>();
                        track.Track = r;
                        //The returned values are fairly basic, so we need to fetch the full track details
                        _ = track.LoadFromId(r.ItemId!, r.Provider!);
                        RecentTracks.Add(track);
                    }

                }),

                Task.Run(async () =>
                {
                    //Discover Albums 
                    //TODO: These should be cached for 12 hours?
                    var albums = await _client.WithWs().GetMusicAlbumsLibraryItemsAsync(limit: 10, order_by: "random");

                    foreach (var a in albums.Result!)
                    {
                        var album = App.Container.Resolve<AlbumViewModel>();
                        album.Album = a;

                        DiscoverAlbums.Add(album);
                    }

                    _logger.LogInformation("Fetching discover albums...");

                }),

                Task.Run(async () =>
                {
                    //Discover Artists
                    //TODO: These should be cached for 12 hours?
                    var artists = await _client.WithWs().GetArtistsAsync(limit: 10, order_by: "random", album_artists_only: true);

                    foreach (var a in artists.Result!)
                    {
                        var artist = App.Container.Resolve<ArtistViewModel>();
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