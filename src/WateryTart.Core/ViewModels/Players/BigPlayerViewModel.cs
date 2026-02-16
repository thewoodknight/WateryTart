using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using UnitsNet;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.ViewModels.Popups;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;
using Xaml.Behaviors.SourceGenerators;

namespace WateryTart.Core.ViewModels.Players;

public partial class BigPlayerViewModel : ReactiveObject, IViewModelBase
{
    private readonly IPlayersService _playersService;
    private double _pendingVolume;
    private System.Timers.Timer? _volumeDebounceTimer;

    public double CachedImageHeight
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double CachedImageWidth
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    [Reactive] public partial IColourService ColourService { get; set; }
    public required IScreen HostScreen { get; set; }
    [Reactive] public partial bool IsLoading { get; set; } = false;
    [Reactive] public partial bool IsSmallDisplay { get; set; }
    public ICommand PlayerNextCommand { get; set; }
    public ICommand PlayerPlayPauseCommand { get; set; }
    public IPlayersService PlayersService => _playersService;
    public ICommand PlayingAltMenuCommand { get; set; }
    public ICommand PlayPreviousCommand { get; set; }
    public ICommand PlayerRepeatTrack { get; set; }
    public ICommand PlayerRepeatQueue { get; set; }
    public ICommand PlayerRepeatOff { get; set; }
    public RelayCommand<double> SeekCommand { get; }

    public ICommand ShowTrackInfo { get; set; }
    public bool ShowBackButton => false;
    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => false;
    public string Title { get; set; } = "";
    public ICommand ToggleFavoriteCommand { get; set; }
    public string? UrlPathSegment { get; } = "BigPlayer";

    public BigPlayerViewModel(IPlayersService playersService, IScreen screen, IColourService colourService)
    {
        ShowTrackInfo = new RelayCommand(() =>
        {
            MessageBus.Current.SendMessage<IPopupViewModel>(new TrackInfoViewModel(PlayersService.SelectedQueue.CurrentItem.StreamDetails));

            Console.WriteLine("got here");
        });

        SeekCommand = new RelayCommand<double>((s) =>
        {
            if (s == 0)
                return;
            var duration = _playersService?.SelectedQueue?.CurrentItem?.Duration;
            var newPosition = duration * (s / 100);
            _playersService?.PlayerSeek(null, (int)newPosition);
        });
        ToggleFavoriteCommand = new RelayCommand(() =>
        {
            var item = PlayersService?.SelectedQueue?.CurrentItem?.MediaItem;
            if (item == null)
                return;
            if (item.Favorite)
                PlayersService.PlayerRemoveFromFavorites(item);
            else
                PlayersService.PlayerAddToFavorites(item);
        });

        PlayerRepeatQueue = new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.All));
        PlayerRepeatOff = new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.Off));
        PlayerRepeatTrack = new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.One));
        PlayPreviousCommand = new RelayCommand(() => PlayersService.PlayerPrevious());
        PlayerNextCommand = new RelayCommand(() => PlayersService.PlayerNext());
        PlayerPlayPauseCommand = new RelayCommand(() => PlayersService.PlayerPlayPause());
        PlayingAltMenuCommand = new RelayCommand(() =>
        {
            var item = PlayersService?.SelectedQueue?.CurrentItem?.MediaItem;

            var GoToAlbum = new RelayCommand(() =>
            {
                if (item == null || HostScreen == null)
                    return;

                var albumVm = App.Container.GetRequiredService<AlbumViewModel>();
                albumVm.Album = item.Album;
                albumVm.LoadFromId(item.Album.ItemId, item.Provider);
                HostScreen.Router.Navigate.Execute(albumVm);
            });

            var GoToArtist = new RelayCommand(() =>
            {
                if (item == null || HostScreen == null)
                    return;
                var artistVm = App.Container.GetRequiredService<ArtistViewModel>();
                artistVm.LoadFromId(item.Artists[0].ItemId, item.Provider);
                HostScreen.Router.Navigate.Execute(artistVm);
            });

            var GoToSimilarTracks = new RelayCommand(() =>
            {
                if (item == null || HostScreen == null)
                    return;
                var SimilarTracksViewModel = App.Container.GetRequiredService<SimilarTracksViewModel>();
                SimilarTracksViewModel.LoadFromId(item.ItemId, item.GetProviderInstance());
                HostScreen.Router.Navigate.Execute(SimilarTracksViewModel);
            });

            var menu = new MenuViewModel(
            [
                new TwoLineMenuItemViewModel("Go to Album", item.Album.Name, MaterialIconKind.Album, GoToAlbum),
                new TwoLineMenuItemViewModel("Go to Artist", item.Artists.FirstOrDefault().Name, MaterialIconKind.Artist, GoToArtist),
                new MenuItemViewModel("Similar tracks", MaterialIconKind.MusicClefTreble, GoToSimilarTracks),
                new MenuItemViewModel("Repeat Mode", MaterialIconKind.Repeat, null),
                new MenuItemViewModel("Repeat Off", MaterialIconKind.RepeatOff, PlayerRepeatOff, true),
                new MenuItemViewModel("Repeat Entire Queue", MaterialIconKind.RepeatVariant, PlayerRepeatQueue, true),
                new MenuItemViewModel("Repeat Single Track", MaterialIconKind.Repeat, PlayerRepeatTrack, true),

            ], PlayersService.SelectedQueue.CurrentItem);
            MessageBus.Current.SendMessage<IPopupViewModel>(menu);
        });
        _playersService = playersService;
        ColourService = colourService;
        HostScreen = screen;

        // Create a CanExecute observable that checks if a player is selected
        var canExecute = this.WhenAnyValue(x => x._playersService.SelectedPlayer)
            .Select(player => player != null)
            .ObserveOn(RxApp.MainThreadScheduler)
            .DistinctUntilChanged();
    }

    public void UpdateCachedDimensions(double width, double height)
    {
        if (width > 0 && height > 0)
        {
            CachedImageWidth = width;
            CachedImageHeight = height;
        }
    }

    [GenerateTypedAction]
    public void VolumeChanged(object sender, object parameter)
    {
        //Debouncing inside itself so it doesnt' get into a loop fighting with MA sending back the new volume
        if (parameter is RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue == args.OldValue)
                return;

            _pendingVolume = args.NewValue;

            _volumeDebounceTimer?.Stop();
            _volumeDebounceTimer?.Dispose();

            _volumeDebounceTimer = new System.Timers.Timer(200);
            _volumeDebounceTimer.AutoReset = false;
            _volumeDebounceTimer.Elapsed += (s, e) =>
            {
                _playersService.PlayerVolume((int)_pendingVolume);
            };
            _volumeDebounceTimer.Start();
        }
    }
}