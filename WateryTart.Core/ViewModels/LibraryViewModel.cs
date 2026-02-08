using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.WebSocketExtensions;

namespace WateryTart.Core.ViewModels;

public partial class LibraryViewModel : ReactiveObject, IViewModelBase, IActivatableViewModel
{
    private ILogger<LibraryViewModel> _logger;
    private readonly IWsClient _massClient;
    public string? UrlPathSegment { get; } = "library";
    public IScreen HostScreen { get; }
    public string Title { get; set; }
    [Reactive] public partial ObservableCollection<LibraryItem> Items { get; set; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
    // CORRECT:
    public ViewModelActivator Activator { get; } = new();

    public LibraryViewModel(IWsClient massClient, IScreen screen, ILoggerFactory loggerFactory)
    {
        _massClient = massClient;
        HostScreen = screen;
        Title = "Library";
        _logger = loggerFactory.CreateLogger<LibraryViewModel>();

        var artists = new LibraryItem()
        {
            Title = "Artists",
            ClickedCommand = new RelayCommand(() =>
            {
                var vm = App.Container.GetRequiredService<ArtistsViewModel>();
                screen.Router.Navigate.Execute(vm);
            })
        };

        var albums = new LibraryItem()
        {
            Title = "Albums",
            ClickedCommand = new RelayCommand(() =>
            {
                var vm = App.Container.GetRequiredService<AlbumsListViewModel>();
                screen.Router.Navigate.Execute(vm);
            })
        };

        var tracks = new LibraryItem()
        {
            Title = "Tracks",
            ClickedCommand = new RelayCommand(() =>
            {
                var vm = App.Container.GetRequiredService<TracksViewModel>();
                screen.Router.Navigate.Execute(vm);
            })
        };
        var playlists = new LibraryItem
        {
            Title = "Playlists",
            ClickedCommand = new RelayCommand(() =>
            {
                var vm = App.Container.GetRequiredService<PlaylistsViewModel>();
                screen.Router.Navigate.Execute(vm);
            })
        };

        //var genres = new LibraryItem { Title = "Genres" };
        var podcasts = new LibraryItem { Title = "Podcasts" };
        var radios = new LibraryItem { Title = "Radios" };
        var audiobooks = new LibraryItem { Title = "Audiobooks" };

        Items =
       [
           artists,
           albums,
           tracks,
           playlists,
          // genres,
           podcasts,
           radios,
           audiobooks
       ];

        // Load counts asynchronously in the background
#pragma warning disable CS4014 // Fire-and-forget intentional - loads data asynchronously
        _ = LoadLibraryCountsAsync(artists, albums, tracks, playlists, null, podcasts, radios, audiobooks);
#pragma warning restore CS4014
    }

    private async Task LoadLibraryCountsAsync(LibraryItem artists, LibraryItem albums, LibraryItem tracks, LibraryItem playlists, LibraryItem genres, LibraryItem podcasts, LibraryItem radios, LibraryItem audiobooks)
    {
        try
        {
            var artistCountResponse = await _massClient.ArtistCountAsync();
            artists.Count = artistCountResponse.Result;

            var albumCountResponse = await _massClient.AlbumsCountAsync();
            albums.Count = albumCountResponse.Result;

            var trackCountResponse = await _massClient.TrackCountAsync();
            tracks.Count = trackCountResponse.Result;

            //Currently the API  docs has this call but it does not return a result
            //var genreCountResponse = await _massClient.GenreCountAsync();
            //genres.Count = genreCountResponse.Result;

            var podcastCountResponse = await _massClient.PodcastCountAsync();
            podcasts.Count = podcastCountResponse.Result;

            var radiosCountResponse = await _massClient.RadiosCountAsync();
            radios.Count = radiosCountResponse.Result;

            var audiobookCountResponse = await _massClient.AudiobookCountAsync();
            audiobooks.Count = audiobookCountResponse.Result;

            var playlistsCountResponse = await _massClient.PlaylistsCountAsync();
            playlists.Count = playlistsCountResponse.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading counts");
        }
    }
}