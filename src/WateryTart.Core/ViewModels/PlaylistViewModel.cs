using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.WsExtensions;
using WateryTart.MusicAssistant.Models;
using CommunityToolkit.Mvvm.Input;
using WateryTart.Core.ViewModels.Popups;

namespace WateryTart.Core.ViewModels
{
    public partial class PlaylistViewModel : ReactiveObject, IViewModelBase
    {
        public string? UrlPathSegment => "playlist";
        public IScreen HostScreen { get; }
        private readonly MusicAssistantClient _massClient;
        private readonly IPlayersService _playersService;
        public bool ShowMiniPlayer => true; 
        public bool ShowNavigation => true;
        [Reactive] public partial Playlist Playlist { get; set; }
        [Reactive] public partial string? Title { get; set; }
        [Reactive] public partial bool IsLoading { get; set; } = false;
        public ObservableCollection<TrackViewModel>? Tracks { get; set; }
        public RelayCommand<Item> PlayCommand { get; }
        public RelayCommand PlaylistAltMenuCommand { get; }
        public RelayCommand PlaylistFullViewCommand { get; }
        public PlaylistViewModel(MusicAssistantClient massClient, IScreen screen, IPlayersService playersService, Playlist? playlist = null)
        {
            _massClient = massClient;
            _playersService = playersService;
            HostScreen = screen;
            Title = "";
            Playlist = playlist ?? new Playlist();

            PlaylistFullViewCommand = new RelayCommand(() =>
            {
                if (Playlist.ItemId != null && Playlist.Provider != null)
                    LoadFromId(Playlist.ItemId, Playlist.Provider);
                screen.Router.Navigate.Execute(this);
            });

            PlaylistAltMenuCommand = new RelayCommand(() =>
            {
                MessageBus.Current.SendMessage<IPopupViewModel>(MenuHelper.BuildStandardPopup(playersService, Playlist));
            });

            PlayCommand = new RelayCommand<Item>((i) => { _playersService.PlayItem(Playlist as MediaItemBase); });
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
                var playlistResponse = await _massClient.WithWs().GetPlaylistAsync(id, provider);
                if (playlistResponse?.Result != null)
                {
                    Playlist = playlistResponse.Result;
                    Title = playlistResponse.Result.Name;
                }
            }
            catch (Exception ex)
            {
                App.Logger?.LogError(ex, $"Error loading playlists");
            }

            try
            {
                var tracksResponse = await _massClient.WithWs().GetPlaylistTracksAsync(id, provider);
                if (tracksResponse?.Result != null)
                {
                    foreach (var t in tracksResponse.Result)
                        Tracks?.Add(new TrackViewModel(_massClient, HostScreen, _playersService, t));
                }
            }
            catch (Exception ex)
            {
                App.Logger?.LogError(ex, $"Error loading playlists tracks");
            }
        }
    }
}

