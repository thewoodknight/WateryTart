using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using WateryTart.Core.Services;

using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;
using WateryTart.Service.MassClient.Responses;

namespace WateryTart.Core.ViewModels
{
    public partial class PlaylistViewModel : ReactiveObject, IViewModelBase
    {
        public string? UrlPathSegment { get; }
        public IScreen HostScreen { get; }
        private readonly IMassWsClient _massClient;
        private readonly IPlayersService _playersService;
        public bool ShowMiniPlayer { get => true; }
        public bool ShowNavigation => true;
        [Reactive] public partial Playlist Playlist { get; set; }
        [Reactive] public partial string Title { get; set; }

        public ObservableCollection<TrackViewModel> Tracks { get; set; }
        public ReactiveCommand<Item, Unit> PlayCommand { get; }
        public ReactiveCommand<Unit, Unit> PlaylistAltMenuCommand { get; }

        public ReactiveCommand<Unit, Unit> PlaylistFullViewCommand { get; }
        public PlaylistViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService, Playlist playlist = null)
        {
            _massClient = massClient;
            _playersService = playersService;
            HostScreen = screen;
            Title = "";
            Playlist = playlist;

            PlaylistFullViewCommand = ReactiveCommand.Create(() =>
            {
                LoadFromId(Playlist.ItemId, Playlist.Provider);
                screen.Router.Navigate.Execute(this);
            });

            PlaylistAltMenuCommand = ReactiveCommand.Create(() =>
            {
                MessageBus.Current.SendMessage(MenuHelper.BuildStandardPopup(playersService, Playlist));
            });

            PlayCommand = ReactiveCommand.Create<Item>((i) =>
            {
                if (Playlist != null)
                    _playersService.PlayItem(Playlist as MediaItemBase);
            });
        }

        public void LoadFromId(string id, string provider)
        {
            Tracks = new ObservableCollection<TrackViewModel>();
#pragma warning disable CS4014 // Fire-and-forget intentional - loads data asynchronously
            _ = LoadPlaylistDataAsync(id, provider);
#pragma warning restore CS4014
        }

        private async Task LoadPlaylistDataAsync(string id, string provider)
        {
            try
            {
                var playlistResponse = await _massClient.PlaylistGetAsync(id, provider);
                if (playlistResponse?.Result != null)
                {
                    Playlist = playlistResponse.Result;
                    Title = playlistResponse.Result.Name;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading playlist: {ex.Message}");
            }

            try
            {
                var tracksResponse = await _massClient.PlaylistTracksGetAsync(id, provider);
                if (tracksResponse?.Result != null)
                {
                    foreach (var t in tracksResponse.Result)
                        Tracks.Add(new TrackViewModel(_massClient, HostScreen, _playersService, t));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading playlist tracks: {ex.Message}");
            }
        }
    }
}

