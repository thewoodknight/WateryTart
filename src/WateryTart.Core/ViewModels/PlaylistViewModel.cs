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
using System.Linq;

namespace WateryTart.Core.ViewModels
{
    public partial class PlaylistViewModel : ViewModelBase<PlaylistViewModel>
    {
        public RelayCommand<Item> PlayCommand { get; }
        public RelayCommand<Item> PlayShuffleCommand { get; }
        
        [Reactive] public partial Playlist Playlist { get; set; }
        public RelayCommand PlaylistAltMenuCommand { get; }
        public RelayCommand PlaylistFullViewCommand { get; }
        public ObservableCollection<TrackViewModel>? Tracks { get; set; }

        private string _runningTime;
        public string RunningTime
        {
            get => _runningTime;
            set => this.RaiseAndSetIfChanged(ref _runningTime, value);
        }
        [Reactive] public override partial bool IsLoading { get; set; }
        public PlaylistViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService, Playlist? playlist = null)
            : base(client: massClient, playersService: playersService)
        {
            HostScreen = screen;
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

            PlayCommand = new RelayCommand<Item>((i) =>
            {
                _ = _playersService?.PlayItem(Playlist as MediaItemBase);
            });

            PlayShuffleCommand = new RelayCommand<Item>((i) =>
            {
                _ = _playersService?.PlayItem(Playlist as MediaItemBase);
                _ = _playersService?.PlayerShuffle(null, true);
            });
        }

        public void LoadFromId(string id, string provider)
        {
            Tracks = [];
            _ = LoadPlaylistDataAsync(id, provider);
        }

        private async Task LoadPlaylistDataAsync(string id, string provider)
        {
            IsLoading = true;
            try
            {
                var playlistResponse = await _client.WithWs().GetPlaylistAsync(id, provider);
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
                var tracksResponse = await _client.WithWs().GetPlaylistTracksAsync(id, provider);
                if (tracksResponse?.Result != null)
                {
                    foreach (var t in tracksResponse.Result)
                        Tracks?.Add(new TrackViewModel(_client, _playersService!, t));
                }

                var totalSeconds = Tracks?.Sum(t => t.Track?.Duration ?? 0) ?? 0;
                var ts = TimeSpan.FromSeconds((int)totalSeconds);
                RunningTime = $"{(int)ts.TotalHours}h {ts.Minutes}m";
            }
            catch (Exception ex)
            {
                App.Logger?.LogError(ex, $"Error loading playlists tracks");
            }

            IsLoading = false;
        }
    }
}