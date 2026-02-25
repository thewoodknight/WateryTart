using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Material;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.ViewModels.Popups;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;
using Xaml.Behaviors.SourceGenerators;
using System.Windows.Input;
namespace WateryTart.Core.ViewModels;

public partial class PlayersViewModel : ReactiveObject, IViewModelBase
{
    [Reactive] public partial PlayersService PlayersService { get; set; }
    public string? UrlPathSegment { get; } = "players";
    public IScreen HostScreen { get; }
    public string Title => "Connect To A Device";
    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => true;
    [Reactive] public partial ObservableCollection<Player> Players { get; set; }
    [Reactive] public partial bool IsLoading { get; set; } = false;
    public Player? SelectedPlayer
    {
        get => field;
        set
        {
            PlayersService.SelectedPlayer = value;
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    [Reactive] public partial RelayCommand<Player> PlayerAltCommand { get; set; }
    [Reactive] public partial RelayCommand<Player> PlayerTogglePlayPauseCommand { get; set; }
    [Reactive] public partial ColourService ColourService { get; set; }

    public ICommand ToggleFavoriteCommand { get; set; }

    public ICommand ToggleShuffleCommand { get; set; }

    public ICommand CycleRepeatCommand { get; set; }

    public ICommand PlayingAltMenuCommand { get; set; }
    public ICommand PlayPreviousCommand { get; set; }
    public ICommand PlayerRepeatTrack { get; set; }
    public ICommand PlayerRepeatQueue { get; set; }
    public ICommand PlayerRepeatOff { get; set; }

    public ICommand PlayerNextCommand { get; set; }
    public ICommand PlayerPlayPauseCommand { get; set; }

    private double _volume;
    private double _pendingVolume;
    private CancellationTokenSource? _volumeCts;
    private bool _suppressVolumeUpdate;

    public double Volume
    {
        get => _volume;
        set => this.RaiseAndSetIfChanged(ref _volume, value);
    }

    public PlayersViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService, ColourService colourService)
    {
        PlayersService = playersService;
        HostScreen = screen;
        Players = playersService.Players;
        SelectedPlayer = playersService.SelectedPlayer;
        ColourService = colourService;

        // Initialize from selected player if available
        Volume = PlayersService?.SelectedPlayer?.VolumeLevel ?? 0;

        // Keep VM Volume in sync with server updates, suppress round-trip
        this.WhenAnyValue(x => x.PlayersService.SelectedPlayer.VolumeLevel)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(serverVol =>
            {
                try
                {
                    _suppressVolumeUpdate = true;
                    Volume = (double)serverVol;
                }
                finally
                {
                    _suppressVolumeUpdate = false;
                }
            });

        PlayerTogglePlayPauseCommand = new RelayCommand<Player>(p =>
        {
#pragma warning disable CS4014
            PlayersService.PlayerPlayPause(p);
#pragma warning restore CS4014
        });
        PlayerAltCommand = new RelayCommand<Player>(p =>
        {
            var menu = new MenuViewModel();
#pragma warning disable CS4014
            if (p.PlaybackState == MusicAssistant.Models.Enums.PlaybackState.Playing)
                menu.AddMenuItem(new MenuItemViewModel("Stop Playback", PackIconMaterialKind.Stop, new RelayCommand(() => PlayersService.PlayerPlayPause(p))));

            var queue = PlayersService.Queues.FirstOrDefault(q => q.QueueId == p.ActiveSource);
            if (queue != null)
            {
                if (!queue.ShuffleEnabled)
                    menu.AddMenuItem(new MenuItemViewModel("Enable shuffle", PackIconMaterialKind.Shuffle, new RelayCommand(() => PlayersService.PlayerShuffle(p))));
                else
                    menu.AddMenuItem(new MenuItemViewModel("Disable shuffle", PackIconMaterialKind.Shuffle, new RelayCommand(() => PlayersService.PlayerShuffle(p, false))));
                var items = new List<MenuItemViewModel>()
                { 
                    //new MenuItemViewModel("Syncronize with another player", PackIconMaterialKind.Link, null),
                    new MenuItemViewModel("Repeat Mode", PackIconMaterialKind.Repeat, null),
                    new MenuItemViewModel("Repeat Off", PackIconMaterialKind.RepeatOff, new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.Off, p)), true),
                    new MenuItemViewModel("Repeat Entire Queue", PackIconMaterialKind.RepeatVariant, new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.All, p)), true),
                    new MenuItemViewModel("Repeat Single Track", PackIconMaterialKind.Repeat, new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.One, p)), true),
                    //  new MenuItemViewModel("Transfer queue", PackIconMaterialKind.Transfer, null),
                    new MenuItemViewModel("Clear queue", PackIconMaterialKind.Cancel, new RelayCommand(() => PlayersService.PlayerClearQueue(p)))
                    // new MenuItemViewModel("Select source", PackIconMaterialKind.Import, null)
                };

                if (!queue.DontStopTheMusicEnabled)
                    menu.AddMenuItem(new MenuItemViewModel("Enable 'Don't stop the music'", PackIconMaterialKind.Infinity, new RelayCommand(() => PlayersService.PlayerDontStopTheMusic(p))));
                else
                    menu.AddMenuItem(new MenuItemViewModel("Disable 'Don't stop the music'", PackIconMaterialKind.Infinity, new RelayCommand(() => PlayersService.PlayerDontStopTheMusic(p, false))));

                menu.AddMenuItem(items);
            }
#pragma warning restore CS4014
            //deep links don't seem to work  new MenuItemViewModel("Open settings", PackIconMaterialKind.Cog,  new RelayCommand(() => App.Launcher.LaunchUriAsync(new System.Uri("http://10.0.1.20:8095/#/settings/editplayer/sendspin-windows-magni")))),
            //new MenuItemViewModel("Open DSP settings", PackIconMaterialKind.Equaliser, null),

            
            MessageBus.Current.SendMessage<IPopupViewModel>(menu);
        });



#pragma warning disable CS4014
        CycleRepeatCommand = new RelayCommand(() =>
        {
            var mode = PlayersService.SelectedQueue.RepeatMode;
            switch (mode)
            {
                case RepeatMode.Off:
                    PlayersService.PlayerSetRepeatMode(RepeatMode.All);
                    break;
                case RepeatMode.All:
                    PlayersService.PlayerSetRepeatMode(RepeatMode.One);
                    break;
                case RepeatMode.One:
                    PlayersService.PlayerSetRepeatMode(RepeatMode.Off);
                    break;
            }
        });
        ToggleShuffleCommand = new RelayCommand(() => PlayersService.PlayerShuffle(null, !PlayersService.SelectedQueue.ShuffleEnabled));
        PlayerRepeatQueue = new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.All));
        PlayerRepeatOff = new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.Off));
        PlayerRepeatTrack = new RelayCommand(() => PlayersService.PlayerSetRepeatMode(RepeatMode.One));
        PlayPreviousCommand = new RelayCommand(() => PlayersService.PlayerPrevious());
        PlayerNextCommand = new RelayCommand(() => PlayersService.PlayerNext());
        PlayerPlayPauseCommand = new RelayCommand(() => PlayersService.PlayerPlayPause());
#pragma warning restore CS4014
    }

    // Add a slider change handler (or use the existing one) that debounces and calls PlayersService
    [GenerateTypedAction]
    public void VolumeChanged(object sender, object parameter)
    {
        if (_suppressVolumeUpdate)
            return;

        if (parameter is RangeBaseValueChangedEventArgs args)
        {
            if (args.NewValue == args.OldValue)
                return;

            _pendingVolume = args.NewValue;

            _volumeCts?.Cancel();
            _volumeCts?.Dispose();
            _volumeCts = new CancellationTokenSource();
            var ct = _volumeCts.Token;
            var pending = _pendingVolume;

            // Determine which player this slider belongs to (sender.DataContext). If not available,
            // fall back to PlayersService.SelectedPlayer in the service method.
            Player? player = null;
            if (sender is Control c && c.DataContext is Player dp)
                player = dp;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(200, ct).ConfigureAwait(false);
                    await PlayersService.PlayerVolume((int)pending, player).ConfigureAwait(false);
                }
                catch (System.OperationCanceledException) { }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Volume change error: {ex}");
                }
            }, ct);
        }
    }

}