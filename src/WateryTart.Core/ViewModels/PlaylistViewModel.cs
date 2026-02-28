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
    public partial class PlaylistViewModel : ViewModelBase<PlaylistViewModel>
    {
        public RelayCommand<Item> PlayCommand { get; }
        [Reactive] public partial Playlist Playlist { get; set; }
        public RelayCommand PlaylistAltMenuCommand { get; }
        public RelayCommand PlaylistFullViewCommand { get; }
        public ObservableCollection<TrackViewModel>? Tracks { get; set; }

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
            }
            catch (Exception ex)
            {
                App.Logger?.LogError(ex, $"Error loading playlists tracks");
            }

            IsLoading = false;
        }
    }
}