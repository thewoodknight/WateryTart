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
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Microsoft.Extensions.Logging;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Responses;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.ViewModels;

public partial class SearchViewModel : ViewModelBase<SearchViewModel>
{
    private readonly CompositeDisposable _disposables;
    [Reactive] public partial string Title { get; set; } = "Search";
    [Reactive] public partial string SearchTerm { get; set; }
    [Reactive] public partial bool IsSearching { get; set; }

    public AsyncRelayCommand SearchCommand { get; }

    public RelayCommand ExpandArtistsResultsCommand { get; }
    public RelayCommand ExpandAlbumResultsCommand { get; }
    public RelayCommand ExpandTracksResultsCommand { get; }
    public RelayCommand ExpandPlaylistResultsCommand { get; }

    private readonly CompositeDisposable _subscriptions = [];
    private readonly SourceList<MediaItemBase> _searchResults = new();
    private readonly ReadOnlyObservableCollection<ArtistViewModel> searchArtists;
    private readonly ReadOnlyObservableCollection<AlbumViewModel> searchAlbums;
    private readonly ReadOnlyObservableCollection<TrackViewModel> searchItem;
    private readonly ReadOnlyObservableCollection<PlaylistViewModel> searchPlaylist;

#pragma warning disable CRR0026 // Unused member
    public ReadOnlyObservableCollection<ArtistViewModel> SearchArtists => searchArtists;
    public ReadOnlyObservableCollection<AlbumViewModel> SearchAlbums => searchAlbums;
    public ReadOnlyObservableCollection<TrackViewModel> SearchItem => searchItem;
    public ReadOnlyObservableCollection<PlaylistViewModel> SearchPlaylist => searchPlaylist;
#pragma warning restore CRR0026 // Unused member

    public SearchViewModel(MusicAssistantClient massclient, ISettings settings, PlayersService playersService, IScreen screen, ILoggerFactory loggerFactory)
        : base(loggerFactory, massclient, playersService)
    {
        _settings = settings;
        HostScreen = screen;
        _disposables = [];

        // Load the last search term
        SearchTerm = _settings.LastSearchTerm ?? string.Empty;

        // Subscribe to search term changes and save them
        this.WhenAnyValue(x => x.SearchTerm)
            .Subscribe(term =>
            {
                if (!string.IsNullOrEmpty(term))
                {
                    _settings.LastSearchTerm = term;
                }
            });

        ExpandArtistsResultsCommand = new RelayCommand(() =>
        {
            var resultsViewModel = App.Container.Resolve<SearchResultsViewModel>();
            resultsViewModel.SetResults(
                _searchResults
                    .Items
                    .Where(i => i is Artist)
                    .Select(i => new ArtistViewModel(_client, HostScreen, playersService, (Artist)i))
            );

            HostScreen.Router.Navigate.Execute(resultsViewModel);
        });
        ExpandAlbumResultsCommand = new RelayCommand(() =>
        {
            var resultsViewModel = App.Container.Resolve<SearchResultsViewModel>();
            resultsViewModel.SetResults(
                _searchResults
                    .Items
                    .Where(i => i is Album)
                    .Select(i => new AlbumViewModel(_client, HostScreen, playersService, (Album)i))
                );
            HostScreen.Router.Navigate.Execute(resultsViewModel);
        });
        ExpandTracksResultsCommand = new RelayCommand(() =>
        {
            var resultsViewModel = App.Container.Resolve<SearchResultsViewModel>();
            resultsViewModel.SetResults(
                _searchResults
                    .Items
                    .Where(i => i is Item)
                    .Select(i => new TrackViewModel(_client, playersService, (Item)i))
            );

            HostScreen.Router.Navigate.Execute(resultsViewModel);
        });
        ExpandPlaylistResultsCommand = new RelayCommand(() =>
        {
            var resultsViewModel = App.Container.Resolve<SearchResultsViewModel>();
            resultsViewModel.SetResults(
                _searchResults
                    .Items
                    .Where(i => i is Playlist)
                    .Select(i => new PlaylistViewModel(_client, HostScreen, playersService, (Playlist)i))
            );

            HostScreen.Router.Navigate.Execute(resultsViewModel);
        });

        // Create a debounced search command
        SearchCommand = new AsyncRelayCommand(async () =>
        {
            IsLoading = true;
            IsSearching = true;
            try
            {
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    SearchResponse results = await (_client.WithWs().SearchAsync(SearchTerm));
                    var searchResponse = results;
                    _searchResults.Clear();
#pragma warning disable CS8604 // Possible null reference argument.
                    _searchResults.AddRange(searchResponse.Result?.Albums);
                    _searchResults.AddRange(searchResponse.Result?.Artists);
                    _searchResults.AddRange(searchResponse.Result?.Playlists);
                    _searchResults.AddRange(searchResponse.Result?.Tracks);
#pragma warning restore CS8604 // Possible null reference argument.
                }
            }
            finally
            {
                IsSearching = false;
                IsLoading = false;
            }
        });

        // Debounce SearchTerm changes and trigger search after 1.5 seconds of inactivity
        this.WhenAnyValue(x => x.SearchTerm)
            .Throttle(TimeSpan.FromSeconds(1.5), RxSchedulers.MainThreadScheduler)
            .Select(_ => Unit.Default)
            .Subscribe(_ => SearchCommand.Execute(null))
            .DisposeWith(_disposables);

        _subscriptions.Add(_searchResults
            .Connect()
            .Filter(i => i is Artist)
            .Transform(i => new ArtistViewModel(_client, screen, playersService, (Artist)i))
            .Top(5)
            .Bind(out searchArtists)
            .Subscribe());

        _subscriptions.Add(_searchResults
            .Connect()
            .Filter(i => i is Album)
            .Transform(i => new AlbumViewModel(_client, screen, playersService, (Album)i))
            .Top(5)
            .Bind(out searchAlbums)
            .Subscribe());

        _subscriptions.Add(_searchResults
            .Connect()
            .Filter(i => i is Item)
            .Transform(i => new TrackViewModel(_client, playersService, (Item)i))
            .Top(5)
            .Bind(out searchItem)
            .Subscribe());

        _subscriptions.Add(_searchResults
            .Connect()
            .Filter(i => i is Playlist)
            .Transform(i => new PlaylistViewModel(_client, screen, playersService, (Playlist)i))
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