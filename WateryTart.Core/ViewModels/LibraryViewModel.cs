using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using WateryTart.Service.MassClient;

namespace WateryTart.Core.ViewModels;

public partial class LibraryViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; set; }
    [Reactive] public partial ObservableCollection<LibraryItem> Items { get; set; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
    public LibraryViewModel(IMassWsClient massClient, IScreen screen)
    {
        _massClient = massClient;
        HostScreen = screen;
        Title = "Library";

        var artists = new LibraryItem()
        {
            Title = "Artists",
            ClickedCommand = ReactiveCommand.Create(() =>
            {
                var vm = App.Container.GetRequiredService<ArtistsViewModel>();
                screen.Router.Navigate.Execute(vm);
            })
        };

        var albums = new LibraryItem()
        {
            Title = "Albums",
            ClickedCommand = ReactiveCommand.Create(() =>
            {
                var vm = App.Container.GetRequiredService<AlbumsListViewModel>();
                screen.Router.Navigate.Execute(vm);
            })
        };

        var tracks = new LibraryItem() { Title = "Tracks" };

        Items =
        [
            artists,
            albums,
            tracks,
            new() { Title = "Playlists" },
            new() { Title = "Genres" }
        ];

        // Load counts asynchronously in the background
#pragma warning disable CS4014 // Fire-and-forget intentional - loads data asynchronously
        _ = LoadLibraryCountsAsync(artists, albums, tracks);
#pragma warning restore CS4014
    }

    private async Task LoadLibraryCountsAsync(LibraryItem artists, LibraryItem albums, LibraryItem tracks)
    {
        try
        {
            var artistCountResponse = await _massClient.ArtistCountAsync();
            artists.Count = artistCountResponse.Result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading artist count: {ex.Message}");
        }

        try
        {
            var albumCountResponse = await _massClient.AlbumsCountAsync();
            albums.Count = albumCountResponse.Result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading album count: {ex.Message}");
        }

        try
        {
            var trackCountResponse = await _massClient.TrackCountAsync();
            tracks.Count = trackCountResponse.Result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading track count: {ex.Message}");
        }
    }
}