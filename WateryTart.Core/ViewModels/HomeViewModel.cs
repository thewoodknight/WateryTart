using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;
using WateryTart.Service.MassClient.Models.Enums;

namespace WateryTart.Core.ViewModels
{
    public class HomeViewModel : ReactiveObject, IViewModelBase
    {
        private readonly ObservableCollection<RecommendationDisplayModel> _displayRecommendations;
        private readonly IMassWsClient _massClient;
        private readonly IPlayersService _playersService;
        private readonly IScreen _screen;
        private readonly ISettings _settings;

        public ReactiveCommand<Item, Unit> AltMenuCommand { get; }
        public ReactiveCommand<Item, Unit> ClickedCommand { get; }
        public IScreen HostScreen { get; }
        public ReactiveCommand<RecommendationDisplayModel, Unit> RecommendationListCommand { get; }
        public ObservableCollection<RecommendationDisplayModel> Recommendations => _displayRecommendations;
        public bool ShowMiniPlayer => true;
        public bool ShowNavigation => true;
        public string Title { get; set; }
        public string? UrlPathSegment { get; }

        private List<Recommendation> SourceRecommendations { get; set; }

        public HomeViewModel(IScreen screen, IMassWsClient massClient, ISettings settings, IPlayersService playersService)
        {
            Title = "Home";
            _massClient = massClient;
            _settings = settings;
            _playersService = playersService;
            _screen = screen;

            _displayRecommendations = new ObservableCollection<RecommendationDisplayModel>();
            SourceRecommendations = new List<Recommendation>();

            RecommendationListCommand = ReactiveCommand.Create<RecommendationDisplayModel>(r =>
            {
                Console.WriteLine($"Navigating to recommendation: {r?.ItemId}");
                var vm = App.Container.GetRequiredService<RecommendationViewModel>();
                vm.SetRecommendation(SourceRecommendations.SingleOrDefault(x => x.ItemId == r.ItemId));
                screen.Router.Navigate.Execute(vm).Subscribe();
            });

            AltMenuCommand = ReactiveCommand.Create<Item>(i =>
            {
                Console.WriteLine($"Creating menu for item: {i?.Name}");

                var playCommand = ReactiveCommand.Create<Unit>(_ =>
                {
                    Console.WriteLine("Play command executed");
                });
                playCommand.ThrownExceptions.Subscribe(ex =>
                    Console.WriteLine($"Play command error: {ex.Message}"));

                var menu = new MenuViewModel(
                [
                    new MenuItemViewModel("Add to library", string.Empty,
                            ReactiveCommand.Create<Unit>(_ => Console.WriteLine("Add to library"))),
                        new MenuItemViewModel("Add to favourites", string.Empty,
                            ReactiveCommand.Create<Unit>(_ => Console.WriteLine("Add to favourites"))),
                        new MenuItemViewModel("Add to playlist", string.Empty,
                            ReactiveCommand.Create<Unit>(_ => Console.WriteLine("Add to playlist"))),
                        new MenuItemViewModel("Play", string.Empty, playCommand)
                ]);

                foreach (var p in _playersService.Players)
                {
                    var player = p; // Capture for closure
                    var playerCommand = ReactiveCommand.CreateFromTask(async () =>
                    {
                        Console.WriteLine($"Playing on {player.DisplayName}");
                        await _playersService.PlayItem(i, player);
                    });
                    playerCommand.ThrownExceptions.Subscribe(ex =>
                        Console.WriteLine($"Player command error: {ex.Message}"));

                    menu.AddMenuItem(new MenuItemViewModel($"\tPlay on {player.DisplayName}", string.Empty, playerCommand));
                }

                MessageBus.Current.SendMessage(menu);
            });

            ClickedCommand = ReactiveCommand.Create<Item>(item =>
            {
                switch (item.MediaType)
                {
                    case MediaType.Album:
                        var albumVm = App.Container.GetRequiredService<AlbumViewModel>();
                        albumVm.Album = item.album;
                        albumVm.LoadFromId(item.ItemId, item.Provider);
                        // ✅ Don't call .Subscribe() here - let the command handle it
                        _screen.Router.Navigate.Execute(albumVm);
                        break;

                    case MediaType.Playlist:
                        var playlistVm = App.Container.GetRequiredService<PlaylistViewModel>();
                        playlistVm.LoadFromId(item.ItemId, item.Provider);
                        _screen.Router.Navigate.Execute(playlistVm);
                        break;

                    case MediaType.Artist:
                        var artistVm = App.Container.GetRequiredService<ArtistViewModel>();
                        artistVm.LoadFromId(item.ItemId, item.Provider);
                        _screen.Router.Navigate.Execute(artistVm);
                        break;

                    default:
                        Console.WriteLine($"Unhandled media type: {item.MediaType}");
                        break;
                }
            });

            Load();
        }

        private async Task Load()
        {
            var recommendationResponse = await _massClient.MusicRecommendationsAsync();
            if (recommendationResponse?.Result == null)
            {
                return;
            }

            var nonEmptyRecommendations = recommendationResponse.Result
                .Where(r => r?.items != null && r.items.Any())
                .ToList();

            foreach (var n in nonEmptyRecommendations)
            {
                SourceRecommendations.Add(n);

                var x = new Recommendation
                {
                    ItemId = n.ItemId,
                    Name = n.Name,
                    MediaType = n.MediaType,
                    items = n.items.Count > 4
                        ? n.items.Take(4).ToList()
                        : new List<Item>(n.items) // Create new list
                };

                // Convert items to appropriate view models based on MediaType
                var viewModels = new List<object>();
                foreach (var item in x.items)
                {
                    IViewModelBase viewModel = item.CreateViewModelForItem();
                    if (viewModel != null)
                        viewModels.Add(viewModel);
                }

                var displayModel = new RecommendationDisplayModel(x, viewModels);
                _displayRecommendations.Add(displayModel);
            }
        }
    }
}