using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WateryTart.Core.Extensions;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
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
        private readonly ILoggerFactory _loggerFactory;
        private readonly ISettings _settings;
        private readonly ILogger<HomeViewModel> _logger;
        public RelayCommand<Item> ClickedCommand { get; }

        public IScreen HostScreen => field;

        public RelayCommand<RecommendationDisplayModel> RecommendationListCommand { get; }
        public ObservableCollection<RecommendationDisplayModel> Recommendations => _displayRecommendations;
        public bool ShowMiniPlayer => true;
        public bool ShowNavigation => true;
        public string Title { get; set; }
        public string? UrlPathSegment => "Home";

        private List<Recommendation> SourceRecommendations { get; set; }

        public HomeViewModel(IScreen screen, IMassWsClient massClient, ISettings settings, IPlayersService playersService, ILoggerFactory loggerFactory)
        {
            Title = "Home";
            _massClient = massClient;
            _settings = settings;
            _playersService = playersService;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<HomeViewModel>();
            HostScreen = screen;

            _displayRecommendations = new ObservableCollection<RecommendationDisplayModel>();
            SourceRecommendations = new List<Recommendation>();

            RecommendationListCommand = new RelayCommand<RecommendationDisplayModel>(r =>
            {
                _logger.LogInformation("Navigating to recommendation: {ItemId}", r?.ItemId);
                if (App.Container != null)
                {
                    var vm = App.Container.GetRequiredService<RecommendationViewModel>();
                    vm.SetRecommendation(SourceRecommendations.SingleOrDefault(x => x.ItemId == r.ItemId));
                    screen.Router.Navigate.Execute(vm).Subscribe();
                }
            });

            ClickedCommand = new RelayCommand<Item>(item =>
            {
                if (App.Container is not null && item is
                    {
                        ItemId: not null, 
                        Provider: not null
                    })
                    switch (item.MediaType)
                    {
                        case MediaType.Album:
                            var albumVm = App.Container.GetRequiredService<AlbumViewModel>();
                            albumVm.Album = item.Album;
                            albumVm.LoadFromId(item.ItemId, item.Provider);
                            HostScreen.Router.Navigate.Execute(albumVm);
                            break;

                        case MediaType.Playlist:
                            var playlistVm = App.Container.GetRequiredService<PlaylistViewModel>();
                            playlistVm.LoadFromId(item.ItemId, item.Provider);
                            HostScreen.Router.Navigate.Execute(playlistVm);
                            break;

                        case MediaType.Artist:
                            var artistVm = App.Container.GetRequiredService<ArtistViewModel>();
                            artistVm.LoadFromId(item.ItemId, item.Provider);
                            HostScreen.Router.Navigate.Execute(artistVm);
                            break;

                        default:
                            _logger.LogWarning("Unhandled media type: {MediaType}", item.MediaType);
                            break;
                    }
            });

            _ = Load();
        }

        private async Task Load()
        {
            var recommendationResponse = await _massClient.MusicRecommendationsAsync();
            if (recommendationResponse?.Result == null)
            {
                return;
            }

            var nonEmptyRecommendations = recommendationResponse.Result
                .Where(r => r?.Items != null && r.Items.Any())
                .ToList();

            foreach (var n in nonEmptyRecommendations)
            {
                SourceRecommendations.Add(n);

                if (n.Items == null)
                    continue;

                var x = new Recommendation
                {
                    ItemId = n.ItemId,
                    Name = n.Name,
                    MediaType = n.MediaType,
                    Items = n.Items.Count > 4
                        ? n.Items.Take(4).ToList()
                        : new List<Item>(n.Items) // Create new list
                };

                // Convert items to appropriate view models based on MediaType
                var viewModels = new List<object>();
                foreach (var item in x.Items)
                {
                    IViewModelBase? viewModel = item.CreateViewModelForItem();
                    if (viewModel != null)
                        viewModels.Add(viewModel);
                }

                var displayModel = new RecommendationDisplayModel(x, viewModels);
                _displayRecommendations.Add(displayModel);
            }
        }
    }
}