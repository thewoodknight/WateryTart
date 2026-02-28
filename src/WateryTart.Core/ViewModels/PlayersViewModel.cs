using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Material;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.ViewModels.Players;
using WateryTart.Core.ViewModels.Popups;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;
using Xaml.Behaviors.SourceGenerators;
using Autofac;
using Microsoft.Extensions.Logging;

namespace WateryTart.Core.ViewModels;

public partial class PlayersViewModel : ViewModelBase<PlayersViewModel>
{
    private double _pendingVolume;
    private bool _suppressVolumeUpdate;
    private double _volume;
    private CancellationTokenSource? _volumeCts;
    [Reactive] public partial RelayCommand<Player> ClickedCommand { get; set; }
    [Reactive] public partial ColourService ColourService { get; set; }
    public ICommand CycleRepeatCommand { get; set; }
    [Reactive] public partial RelayCommand<Player> PlayerAltCommand { get; set; }
    public ICommand PlayerNextCommand { get; set; }
    public ICommand PlayerPlayPauseCommand { get; set; }
    public ICommand PlayerRepeatOff { get; set; }
    public ICommand PlayerRepeatQueue { get; set; }
    public ICommand PlayerRepeatTrack { get; set; }
    [Reactive] public partial ObservableCollection<Player> Players { get; set; }
    [Reactive] public partial RelayCommand<Player> PlayerTogglePlayPauseCommand { get; set; }

    public ICommand PlayPreviousCommand { get; set; }

    public Player? SelectedPlayer
    {
        get => field;
        set
        {
            _playersService?.SelectedPlayer = value;
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    public ICommand ToggleShuffleCommand { get; set; }

    public double Volume
    {
        get => _volume;
        set => this.RaiseAndSetIfChanged(ref _volume, value);
    }

    public PlayersViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService, ColourService colourService, ILoggerFactory loggerFactory)
        : base(client: massClient, playersService: playersService, screen: screen, loggerFactory: loggerFactory)
    {
        ShowMiniPlayer = false;
        Title = "Connect To A Device";
        Players = _playersService!.Players;
        SelectedPlayer = _playersService.SelectedPlayer;
        ColourService = colourService;
        Volume = _playersService?.SelectedPlayer?.VolumeLevel ?? 0;

        // Setup commands
        ClickedCommand = new RelayCommand<Player>(p =>
        {
            var vm = App.Container?.Resolve<BigPlayerViewModel>();
            if (vm != null)
                HostScreen.Router.Navigate.Execute(vm);
        });

        PlayerTogglePlayPauseCommand = new RelayCommand<Player>(p =>
        {
            _ = _playersService?.PlayerPlayPause(p);
        });

        PlayerAltCommand = new RelayCommand<Player>(p =>
        {
            var menu = new MenuViewModel();
            if (p?.PlaybackState == PlaybackState.Playing)
                menu.AddMenuItem(new MenuItemViewModel("Stop Playback", PackIconMaterialKind.Stop, new RelayCommand(() => _ = _playersService!.PlayerPlayPause(p))));

            var queue = _playersService?.Queues.FirstOrDefault(q => q.QueueId == p.ActiveSource);
            if (queue != null)
            {
                if (!queue.ShuffleEnabled)
                    menu.AddMenuItem(new MenuItemViewModel("Enable shuffle", PackIconMaterialKind.Shuffle, new RelayCommand(() => _ = _playersService!.PlayerShuffle(p))));
                else
                    menu.AddMenuItem(new MenuItemViewModel("Disable shuffle", PackIconMaterialKind.Shuffle, new RelayCommand(() => _ = _playersService!.PlayerShuffle(p, false))));
                var items = new List<MenuItemViewModel>()
                {
                    new("Repeat Mode", PackIconMaterialKind.Repeat, null),
                    new("Repeat Off", PackIconMaterialKind.RepeatOff, new RelayCommand(() =>  _ = _playersService!.PlayerSetRepeatMode(RepeatMode.Off, p)), true),
                    new("Repeat Entire Queue", PackIconMaterialKind.RepeatVariant, new RelayCommand(() =>  _ = _playersService!.PlayerSetRepeatMode(RepeatMode.All, p)), true),
                    new("Repeat Single Track", PackIconMaterialKind.Repeat, new RelayCommand(() =>  _ = _playersService!.PlayerSetRepeatMode(RepeatMode.One, p)), true),
                    new("Clear queue", PackIconMaterialKind.Cancel, new RelayCommand(() =>  _ = _playersService!.PlayerClearQueue(p)))
                };

                if (!queue.DontStopTheMusicEnabled)
                    menu.AddMenuItem(new MenuItemViewModel("Enable 'Don't stop the music'", PackIconMaterialKind.Infinity, new RelayCommand(() => _ = _playersService!.PlayerDontStopTheMusic(p))));
                else
                    menu.AddMenuItem(new MenuItemViewModel("Disable 'Don't stop the music'", PackIconMaterialKind.Infinity, new RelayCommand(() => _ = _playersService!.PlayerDontStopTheMusic(p, false))));

                menu.AddMenuItem(items);
            }

            MessageBus.Current.SendMessage<IPopupViewModel>(menu);
        });

        CycleRepeatCommand = new RelayCommand(() =>
        {
            var mode = _playersService?.SelectedQueue?.RepeatMode;
            switch (mode)
            {
                case RepeatMode.Off:
                    _ = _playersService?.PlayerSetRepeatMode(RepeatMode.All);
                    break;

                case RepeatMode.All:
                    _ = _playersService?.PlayerSetRepeatMode(RepeatMode.One);
                    break;

                case RepeatMode.One:
                    _ = _playersService?.PlayerSetRepeatMode(RepeatMode.Off);
                    break;
            }
        });

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        ToggleShuffleCommand = new RelayCommand(() => _playersService.PlayerShuffle(null, !_playersService.SelectedQueue.ShuffleEnabled));
        PlayerRepeatQueue = new RelayCommand(() => _playersService.PlayerSetRepeatMode(RepeatMode.All));
        PlayerRepeatOff = new RelayCommand(() => _playersService.PlayerSetRepeatMode(RepeatMode.Off));
        PlayerRepeatTrack = new RelayCommand(() => _playersService.PlayerSetRepeatMode(RepeatMode.One));
        PlayPreviousCommand = new RelayCommand(() => _playersService.PlayerPrevious());
        PlayerNextCommand = new RelayCommand(() => _playersService.PlayerNext());
        PlayerPlayPauseCommand = new RelayCommand(() => _playersService.PlayerPlayPause());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        // Keep VM Volume in sync with server updates, suppress round-trip
        this.WhenAnyValue(x => x._playersService!.SelectedPlayer!.VolumeLevel)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(serverVol =>
            {
                try
                {
                    _suppressVolumeUpdate = true;
                    Volume = (double)serverVol!;
                }
                finally
                {
                    _suppressVolumeUpdate = false;
                }
            });
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
                    await _playersService!.PlayerVolume((int)pending, player).ConfigureAwait(false);
                }
                catch (OperationCanceledException) 
                { 
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Volume change error: {ex}");
                }
            }, ct);
        }
    }
}