using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly PlayersService _playersService;
    private double _pendingVolume;
    private CancellationTokenSource? _volumeCts;
    private bool _suppressVolumeUpdate;
    private double _volume; // backing for exposed Volume property

    private static readonly HashSet<string> _losslessContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "FLAC",
        "AIFF",
        "WAV",
        "ALAC",
        "WAVPACK",
        "TAK",
        "APE",
        "TRUEHD",
        "DSD_LSBF",
        "DSD_MSBF",
        "DSD_LSBF_PLANAR",
        "DSD_MSBF_PLANAR",
        "RA_144"
    };

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

    // Expose a VM property for slider binding. This is the single source for UI -> PlayersService.
    public double Volume
    {
        get => _volume;
        set => this.RaiseAndSetIfChanged(ref _volume, value);
    }

    [Reactive] public partial ColourService ColourService { get; set; }
    public required IScreen HostScreen { get; set; }
    [Reactive] public partial bool IsLoading { get; set; } = false;
    [Reactive] public partial bool IsSmallDisplay { get; set; }
    public ICommand PlayerNextCommand { get; set; }
    public ICommand PlayerPlayPauseCommand { get; set; }
    public PlayersService PlayersService => _playersService;
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

    public BigPlayerViewModel(PlayersService playersService, IScreen screen, ColourService colourService)
    {
        _playersService = playersService;
        ColourService = colourService;
        HostScreen = screen;

        // Initialize VM Volume from currently selected player if available
        Volume = _playersService?.SelectedPlayer?.VolumeLevel ?? 0;

        // Keep VM Volume in sync with PlayersService.SelectedPlayer.VolumeLevel
        // When a server update drives the player model, update the VM's Volume but suppress sending it back to server.
        this.WhenAnyValue(x => x.PlayersService.SelectedPlayer.VolumeLevel)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(serverVol =>
            {
                try
                {
                    _suppressVolumeUpdate = true;
                    Volume = (int)serverVol;
                }
                finally
                {
                    _suppressVolumeUpdate = false;
                }
            });

        ShowTrackInfo = new RelayCommand(() =>
        {
            if (PlayersService != null && PlayersService.SelectedQueue != null && PlayersService.SelectedQueue.CurrentItem != null && PlayersService.SelectedPlayer != null)
                MessageBus.Current.SendMessage<IPopupViewModel>(new TrackInfoViewModel(PlayersService.SelectedQueue.CurrentItem, PlayersService.SelectedPlayer));
        });

        SeekCommand = new RelayCommand<double>((s) =>
        {
            if (s == 0)
                return;
            var duration = _playersService?.SelectedQueue?.CurrentItem?.Duration;
            var newPosition = duration * (s / 100);
            if (newPosition != null)
                _playersService?.PlayerSeek(null, (int)newPosition);
        });

        ToggleFavoriteCommand = new RelayCommand(() =>
        {
            var item = PlayersService?.SelectedQueue?.CurrentItem?.MediaItem;
            if (item == null)
                return;

            if (item.Favorite)
                PlayersService?.PlayerRemoveFromFavorites(item);
            else
                PlayersService?.PlayerAddToFavorites(item);
        });

#pragma warning disable CS4014
        PlayerRepeatQueue = new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.All));
        PlayerRepeatOff = new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.Off));
        PlayerRepeatTrack = new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.One));
        PlayPreviousCommand = new RelayCommand(() => PlayersService.PlayerPrevious());
        PlayerNextCommand = new RelayCommand(() => PlayersService.PlayerNext());
        PlayerPlayPauseCommand = new RelayCommand(() => PlayersService.PlayerPlayPause());
#pragma warning restore CS4014

        PlayingAltMenuCommand = new RelayCommand(() =>
        {
            var item = PlayersService?.SelectedQueue?.CurrentItem?.MediaItem;

            var GoToAlbum = new RelayCommand(() =>
            {
                if (item == null || item.Album == null || item.Album.ItemId == null || item.Provider == null || HostScreen == null)
                    return;

                var albumVm = App.Container.GetRequiredService<AlbumViewModel>();
                albumVm.Album = item.Album;
                albumVm.LoadFromId(item.Album.ItemId, item.Provider);
                HostScreen.Router.Navigate.Execute(albumVm);
            });

            var GoToArtist = new RelayCommand(() =>
            {
                if (item == null || item.Artists == null || item.Provider == null || HostScreen == null)
                    return;
                var artist = item.Artists.FirstOrDefault();
                if (artist == null || artist.ItemId == null)
                    return;

                var artistVm = App.Container.GetRequiredService<ArtistViewModel>();
                artistVm.LoadFromId(artist.ItemId, item.Provider);
                HostScreen.Router.Navigate.Execute(artistVm);
            });

            var GoToSimilarTracks = new RelayCommand(() =>
            {
                if (item == null || HostScreen == null || item.ItemId == null)
                    return;
                var SimilarTracksViewModel = App.Container.GetRequiredService<SimilarTracksViewModel>();
#pragma warning disable CS4014
                SimilarTracksViewModel.LoadFromId(item.ItemId, item.GetProviderInstance());
#pragma warning restore CS4014
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

        // Create a CanExecute observable that checks if a player is selected
        var canExecute = this.WhenAnyValue(x => x._playersService.SelectedPlayer)
            .Select(player => player != null)
            .ObserveOn(RxApp.MainThreadScheduler)
            .DistinctUntilChanged();

#pragma warning disable CS8602
        // Ensure Quality property updates when the selected queue item or player changes
        this.WhenAnyValue(x => x.PlayersService.SelectedQueue.CurrentItem)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(Quality)));

        // Also update Quality when inner stream details change (bit depth, sample rate or content type)
        this.WhenAnyValue(
                x => x.PlayersService.SelectedQueue.CurrentItem.StreamDetails.AudioFormat.BitDepth,
                x => x.PlayersService.SelectedQueue.CurrentItem.StreamDetails.AudioFormat.SampleRate,
                x => x.PlayersService.SelectedQueue.CurrentItem.StreamDetails.AudioFormat.ContentType)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(Quality)));
#pragma warning restore CS8602
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
        // Debounce slider changes and route through PlayersService's coordinated setter.
        // Ignore changes that were driven by server updates (suppressed).
        if (_suppressVolumeUpdate)
            return;

        if (parameter is RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue == args.OldValue)
                return;

            _pendingVolume = args.NewValue;

            // Cancel previous pending task and schedule a new debounce task.
            _volumeCts?.Cancel();
            _volumeCts?.Dispose();
            _volumeCts = new CancellationTokenSource();
            var ct = _volumeCts.Token;
            var pending = _pendingVolume;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(200, ct).ConfigureAwait(false);
                    // Use PlayersService.PlayerVolume which serializes and performs echo suppression.
                    await _playersService.PlayerVolume((int)pending).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Volume change error: {ex}");
                }
            }, ct);
        }
    }

    private bool IsContentTypeLossless(string contentType)
    {
        return _losslessContentTypes.Contains(contentType);
    }

    public QualityTier Quality
    {
        get
        {
            var streamDetails = PlayersService?.SelectedQueue?.CurrentItem?.StreamDetails;

            if (streamDetails == null || streamDetails.AudioFormat == null || string.IsNullOrEmpty(streamDetails.AudioFormat.ContentType))
                return QualityTier.LOW;

            if (streamDetails.AudioFormat.BitDepth > 16 || streamDetails.AudioFormat.SampleRate > 48000)
            {
                return QualityTier.HIRES;
            }
            else if (IsContentTypeLossless(streamDetails.AudioFormat.ContentType))
            {
                return QualityTier.HQ;
            }
            else
            {
                return QualityTier.LOW;
            }
        }
    }
}

public enum QualityTier
{
    LOW,
    HQ,
    HIRES
}