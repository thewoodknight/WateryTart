using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Reactive;
using WateryTart.MassClient;
using WateryTart.MassClient.Models;
using WateryTart.MassClient.Responses;
using WateryTart.Services;

namespace WateryTart.ViewModels
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

        public ObservableCollection<Item> Tracks { get; set; }
        public ReactiveCommand<Item, Unit> PlayCommand { get; }

        public PlaylistViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService)
        {
            _massClient = massClient;
            _playersService = playersService;
            HostScreen = screen;
            Title = "";

            PlayCommand = ReactiveCommand.Create<Item>((i) =>
            {
                _playersService.PlayItem(Playlist);
            });
        }

        public void LoadFromId(string id, string provider)
        {
            Tracks = new ObservableCollection<Item>();

            _massClient.PlaylistTracksGet(id, provider, TrackListHandler);
            _massClient.PlaylistGet(id, provider, PlaylistHandler);
        }

        private void PlaylistHandler(PlaylistResponse? response)
        {
            if (response.Result == null)
                return;

            this.Playlist = response.Result;
            Title = Playlist.Name;
        }

        public void TrackListHandler(TracksResponse? response)
        {
            if (response.Result == null)
                return;

            foreach (var t in response.Result)
                Tracks.Add(t);
        }
    }
}