using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Autofac;
using DynamicData;
using Splat;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.Views;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;
using WateryTart.Service.MassClient.Responses;

namespace WateryTart.Core.ViewModels;

public partial class SearchViewModel : ReactiveObject, IViewModelBase, IAsyncReaper
{
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;
    private readonly CompositeDisposable _disposables;

    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public bool ShowMiniPlayer { get => true; }
    public bool ShowNavigation => true;

    public string Title
    {
        get => "Search";
        set;
    }

    [Reactive] public partial string SearchTerm { get; set; }
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    public ReactiveCommand<Unit, Unit> ExpandArtistsResultsCommand { get; }
    public ReactiveCommand<Unit, Unit> ExpandAlbumResultsCommand { get; }
    public ReactiveCommand<Unit, Unit> ExpandTracksResultsCommand { get; }
    public ReactiveCommand<Unit, Unit> ExpandPlaylistResultsCommand { get; }


    private SearchResponse _searchResponse { get; set; }
    private CompositeDisposable _subscriptions = new CompositeDisposable();

    SourceList<MediaItemBase> _searchResults = new SourceList<MediaItemBase>();

    private ReadOnlyObservableCollection<ArtistViewModel> searchArtists;
    private ReadOnlyObservableCollection<AlbumViewModel> searchAlbums;
    private ReadOnlyObservableCollection<TrackViewModel> searchItem;
    private ReadOnlyObservableCollection<PlaylistViewModel> searchPlaylist;

    public ReadOnlyObservableCollection<ArtistViewModel> SearchArtists => searchArtists;
    public ReadOnlyObservableCollection<AlbumViewModel> SearchAlbums => searchAlbums;
    public ReadOnlyObservableCollection<TrackViewModel> SearchItem => searchItem;
    public ReadOnlyObservableCollection<PlaylistViewModel> SearchPlaylist => searchPlaylist;

    public SearchViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        _disposables = [];

        //PlayAlbumCommand = ReactiveCommand.Create(() => _playersService.PlayItem(Album, mode: PlayMode.Replace));

        ExpandArtistsResultsCommand = ReactiveCommand.Create(() =>
        {
            var resultsViewModel = App.Container.Resolve<SearchResultsViewModel>();
            resultsViewModel.SetResults(
                _searchResults
                    .Items
                    .Where(i => i is Artist)
                    .Select(i => new ArtistViewModel(massClient, HostScreen, playersService, (Artist)i))
            );

            HostScreen.Router.Navigate.Execute(resultsViewModel);

        });
        ExpandAlbumResultsCommand = ReactiveCommand.Create(() =>
        {
            var resultsViewModel = App.Container.Resolve<SearchResultsViewModel>();
            resultsViewModel.SetResults(
                _searchResults
                    .Items
                    .Where(i => i is Album)
                    .Select(i => new AlbumViewModel(massClient, HostScreen, playersService, (Album)i) )
                );
            HostScreen.Router.Navigate.Execute(resultsViewModel);
        });
        ExpandTracksResultsCommand = ReactiveCommand.Create(() =>
        {
            var resultsViewModel = App.Container.Resolve<SearchResultsViewModel>();
            resultsViewModel.SetResults(
                _searchResults
                    .Items
                    .Where(i => i is Item)
                    .Select(i => new TrackViewModel(massClient, HostScreen, playersService, (Item)i))
            );

            HostScreen.Router.Navigate.Execute(resultsViewModel);
        });
        ExpandPlaylistResultsCommand = ReactiveCommand.Create(() =>
        {
            var resultsViewModel = App.Container.Resolve<SearchResultsViewModel>();
            resultsViewModel.SetResults(
                _searchResults
                    .Items
                    .Where(i => i is Playlist)
                    .Select(i => new PlaylistViewModel(massClient, HostScreen, playersService, (Playlist)i))
            );

            HostScreen.Router.Navigate.Execute(resultsViewModel);
        });

        // Create a debounced search command
        SearchCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                SearchResponse results = await (_massClient.SearchAsync(SearchTerm));
                _searchResponse = results;
                _searchResults.Clear();
                _searchResults.AddRange(_searchResponse.Result.albums);
                _searchResults.AddRange(_searchResponse.Result.artists);
                _searchResults.AddRange(_searchResponse.Result.playlists);
                _searchResults.AddRange(_searchResponse.Result.tracks);
            }
        });

        // Debounce SearchTerm changes and trigger search after 1.5 seconds of inactivity
        this.WhenAnyValue(x => x.SearchTerm)
            .Throttle(TimeSpan.FromSeconds(1.5), RxApp.MainThreadScheduler)
            .Select(_ => Unit.Default)
            .InvokeCommand(SearchCommand)
            .DisposeWith(_disposables);


        _subscriptions.Add(_searchResults
            .Connect()
            .Filter(i => i is Artist)
            .Transform(i => new ArtistViewModel(massClient, screen, playersService, (Artist)i))
            .Top(5)
            .Bind(out searchArtists)
            .Subscribe());

        _subscriptions.Add(_searchResults
            .Connect()
            .Filter(i => i is Album)
            .Transform(i => new AlbumViewModel(massClient, screen, playersService, (Album)i))
            .Top(5)
            .Bind(out searchAlbums)
            .Subscribe());

        _subscriptions.Add(_searchResults
            .Connect()
            .Filter(i => i is Item)
            .Transform(i => new TrackViewModel(massClient, screen, playersService, (Item)i))
            .Top(5)
            .Bind(out searchItem)
            .Subscribe());

        _subscriptions.Add(_searchResults
            .Connect()
            .Filter(i => i is Playlist)
            .Transform(i => new PlaylistViewModel(massClient, screen, playersService, (Playlist)i))
            .Top(5)
            .Bind(out searchPlaylist)
            .Subscribe());
    }

    public void Reap()
    {
        _subscriptions?.Dispose();
    }

    public async Task ReapAsync()
    {
        _subscriptions?.Dispose();
    }
}