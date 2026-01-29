using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Runtime.Serialization;
using WateryTart.MassClient;
using WateryTart.MassClient.Models;
using WateryTart.MassClient.Responses;
using WateryTart.Services;
using WateryTart.Settings;
using WateryTart.ViewModels.Menus;

namespace WateryTart.ViewModels
{
    public class HomeViewModel : ReactiveObject, IViewModelBase
    {
        private readonly IMassWsClient _massClient;
        private readonly ISettings _settings;
        private readonly IPlayersService _playersService;
        private readonly IScreen _screen;
        public ObservableCollection<Recommendation> Recommendations { get; set; }
        private List<Recommendation> SourceRecommendations { get; set; }
        public bool ShowMiniPlayer => true;
        public bool ShowNavigation => true;
        public ReactiveCommand<Item, Unit> ClickedCommand { get; }
        public ReactiveCommand<Item, Unit> AltMenuCommand { get; }
        public ReactiveCommand<Recommendation, Unit> RecommendationListCommand { get; }

        public HomeViewModel(IScreen screen, IMassWsClient massClient, ISettings settings, IPlayersService playersService)
        {
            Title = "Home";
            _massClient = massClient;
            _settings = settings;
            _playersService = playersService;
            _screen = screen;

            Recommendations = new ObservableCollection<Recommendation>();
            SourceRecommendations = new List<Recommendation>();
            _massClient.MusicRecommendations(RecommendationHandler);

            RecommendationListCommand = ReactiveCommand.Create<Recommendation>(r =>
            {
                var vm = WateryTart.App.Container.GetRequiredService<RecommendationViewModel>();
                vm.SetRecommendation(SourceRecommendations.SingleOrDefault(x => x.ItemId == r.ItemId));
                screen.Router.Navigate.Execute(vm);
            });

            AltMenuCommand = ReactiveCommand.Create<Item>(i =>
            {
                var menu = new MenuViewModel();
                menu.AddMenuItem(new MenuItemViewModel("Add to library", string.Empty, ReactiveCommand.Create<Unit>(r =>
                {
                    Debug.WriteLine("got here");
                })));
                menu.AddMenuItem(new MenuItemViewModel("Add to favourites", string.Empty, ReactiveCommand.Create<Unit>(r =>
                {
                    Debug.WriteLine("got here");
                })));
                menu.AddMenuItem(new MenuItemViewModel("Add to playlist", string.Empty, ReactiveCommand.Create<Unit>(r =>
                {
                    Debug.WriteLine("got here");
                })));
                menu.AddMenuItem(new MenuItemViewModel("Play", string.Empty, ReactiveCommand.Create<Unit>(r =>
                {
                    Debug.WriteLine("got here");
                })));

                foreach (var p in _playersService.Players)
                {
                    menu.AddMenuItem(new MenuItemViewModel($"\tPlay on {p.DisplayName}", string.Empty, ReactiveCommand.Create<Unit>(r =>
                    {
                        _playersService.PlayItem(i, p);
                    })));
                }

                MessageBus.Current.SendMessage(menu);
            });

            ClickedCommand = ReactiveCommand.Create<Item>(item =>
            {
                var i = item; //navigate to whatever

                switch (i.MediaType)
                {
                    case MediaType.Album:

                        var vm = WateryTart.App.Container.GetRequiredService<AlbumViewModel>();
                        vm.Album = item.album;
                        vm.LoadFromId(item.ItemId, item.Provider);
                        screen.Router.Navigate.Execute(vm);
                        break;

                    case MediaType.Playlist:
                        var playlistViewModel = WateryTart.App.Container.GetRequiredService<PlaylistViewModel>();
                        playlistViewModel.LoadFromId(item.ItemId, item.Provider);
                        screen.Router.Navigate.Execute(playlistViewModel);
                        break;

                    case MediaType.Artist:
                        var artistViewModel = WateryTart.App.Container.GetRequiredService<ArtistViewModel>();
                        artistViewModel.LoadFromId(item.ItemId, item.Provider);
                        screen.Router.Navigate.Execute(artistViewModel);
                        break;

                    case MediaType.Genre:
                        break;

                    case MediaType.Radio: break;
                    case MediaType.Track: break;

                    case MediaType.Audiobook: break;
                    case MediaType.Folder: break;
                    case MediaType.Podcast: break;
                    case MediaType.PodcastEpisode: break;
                }
            });
        }

        public void RecommendationHandler(RecommendationResponse response)
        {
            var nonEmptyRecommendations = response
                .Result
                .Where(r => r.items.Any());

            foreach (var n in nonEmptyRecommendations)
            {
                SourceRecommendations.Add(n); //C# is byref, not byval, so would have to clone it.
                // https://docs.avaloniaui.net/docs/concepts/reactiveui/binding-to-sorted-filtered-list
                // Theoretically dynamicdata should be used with a SourceList, but a copied list of view models may be better suited.

                Recommendation x = (Recommendation)CustomMemberwiseClone(n);
                if (x.items.Count > 4)
                    x.items = x.items.GetRange(0, 4);

                Recommendations.Add(x);
            }
        }

        public string? UrlPathSegment { get; }
        public IScreen HostScreen { get; }
        public string Title { get; set; }

        public static object CustomMemberwiseClone(object source)
        {
            var clone = FormatterServices.GetUninitializedObject(source.GetType());
            for (var type = source.GetType(); type != null; type = type.BaseType)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                    field.SetValue(clone, field.GetValue(source));
            }
            return clone;
        }
    }
}