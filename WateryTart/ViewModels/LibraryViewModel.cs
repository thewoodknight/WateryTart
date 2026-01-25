using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;
using WateryTart.MassClient;
using WateryTart.MassClient.Models;

namespace WateryTart.ViewModels;

public partial class LibraryViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    [Reactive] public string Title { get; set; }
    [Reactive] public partial ObservableCollection<LibraryItem> Items { get; set; }

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
                var vm = WateryTart.App.Container.GetRequiredService<ArtistsViewModel>();
                screen.Router.Navigate.Execute(vm);
            })

        };
        massClient.ArtistCount((a) =>
        {
            artists.Count = a.Result;
        });


        var albums = new LibraryItem()
        {
            Title = "Albums",
            ClickedCommand = ReactiveCommand.Create(() =>
            {
                var vm = WateryTart.App.Container.GetRequiredService<AlbumsListViewModel>();
                screen.Router.Navigate.Execute(vm);
            })
        };
        massClient.AlbumsCount((a) =>
        {
            albums.Count = a.Result;
        });

        var tracks = new LibraryItem() { Title = "Tracks" };
        massClient.TrackCount((a) =>
        {
            tracks.Count = a.Result;
        });



        Items =
        [
            artists,
            albums,
            tracks,
            new() { Title = "Playlists" },
            new() { Title = "Genres" }
        ];

        /*{
             "command": "music/artists/count",
             "args": {
               "favorite_only": false,
               "album_artists_only": true
             }
           }


         {
             "command": "music/albums/count",
             "args": {
               "favorite_only": false,
               "album_types": ["album", "single", "live", "soundtrack", "compilation", "ep", "unknown"]
             }
           }

         {
             "command": "music/playlists/count",
             "args": {
               "favorite_only": false
             }
           }

{
             "command": "music/tracks/count",
             "args": {
               "favorite_only": false
             }
           }
         */
    }
}

public partial class LibraryItem : ReactiveObject
{
    public string Title { get; set; }
    public string LowerTitle
    {
        get { return Title.ToLowerInvariant(); }
    }
    [Reactive] public partial int Count { get; set; }
    public ReactiveCommand<Unit, Unit> ClickedCommand { get; set; }
}