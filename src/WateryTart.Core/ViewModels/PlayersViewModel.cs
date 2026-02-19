using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.ViewModels.Popups;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;
using Xaml.Behaviors.SourceGenerators;

namespace WateryTart.Core.ViewModels;

public partial class PlayersViewModel : ReactiveObject, IViewModelBase
{
    private readonly PlayersService _playersService;
    public string? UrlPathSegment { get; } = "players";
    public IScreen HostScreen { get; }
    public string Title => "Players";
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
    [Reactive] public partial ObservableCollection<Player> Players { get; set; }
    [Reactive] public partial bool IsLoading { get; set; } = false;
    public Player? SelectedPlayer
    {
        get => field;
        set
        {
            _playersService.SelectedPlayer = value;
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    [Reactive] public partial RelayCommand<Player> PlayerAltCommand { get; set; }
    [Reactive] public partial RelayCommand<Player> PlayerTogglePlayPauseCommand { get; set; }
    [Reactive] public partial ColourService ColourService { get; set; }
    public PlayersViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService, ColourService colourService)
    {
        _playersService = playersService;
        HostScreen = screen;
        Players = playersService.Players;
        SelectedPlayer = playersService.SelectedPlayer;
        ColourService = colourService;

        PlayerTogglePlayPauseCommand = new RelayCommand<Player>(p =>
        {
#pragma warning disable CS4014
            _playersService.PlayerPlayPause(p);
#pragma warning restore CS4014
        });
        PlayerAltCommand = new RelayCommand<Player>(p =>
        {
            var menu = new MenuViewModel();
#pragma warning disable CS4014
            if (p.PlaybackState == MusicAssistant.Models.Enums.PlaybackState.Playing)
                menu.AddMenuItem(new MenuItemViewModel("Stop Playback", MaterialIconKind.Stop, new RelayCommand(() => _playersService.PlayerPlayPause(p))));

            var queue = _playersService.Queues.FirstOrDefault(q => q.QueueId == p.ActiveSource);
            if (queue != null)
            {
                if (!queue.ShuffleEnabled)
                    menu.AddMenuItem(new MenuItemViewModel("Enable shuffle", MaterialIconKind.Shuffle, new RelayCommand(() => _playersService.PlayerShuffle(p))));
                else
                    menu.AddMenuItem(new MenuItemViewModel("Disable shuffle", MaterialIconKind.Shuffle, new RelayCommand(() => _playersService.PlayerShuffle(p, false))));
                var items = new List<MenuItemViewModel>()
                { 
                    //new MenuItemViewModel("Syncronize with another player", MaterialIconKind.Link, null),
                    new MenuItemViewModel("Repeat Mode", MaterialIconKind.Repeat, null),
                    new MenuItemViewModel("Repeat Off", MaterialIconKind.RepeatOff, new RelayCommand(() => _playersService.PlayerSetRepeatMode(RepeatMode.Off, p)), true),
                    new MenuItemViewModel("Repeat Entire Queue", MaterialIconKind.RepeatVariant, new RelayCommand(() => _playersService.PlayerSetRepeatMode(RepeatMode.All, p)), true),
                    new MenuItemViewModel("Repeat Single Track", MaterialIconKind.Repeat, new RelayCommand(() => _playersService.PlayerSetRepeatMode(RepeatMode.One, p)), true),
                    //  new MenuItemViewModel("Transfer queue", MaterialIconKind.Transfer, null),
                    new MenuItemViewModel("Clear queue", MaterialIconKind.Cancel, new RelayCommand(() => _playersService.PlayerClearQueue(p)))
                    // new MenuItemViewModel("Select source", MaterialIconKind.Import, null)
                };

                if (!queue.DontStopTheMusicEnabled)
                    menu.AddMenuItem(new MenuItemViewModel("Enable 'Don't stop the music'", MaterialIconKind.Infinity, new RelayCommand(() => _playersService.PlayerDontStopTheMusic(p))));
                else
                    menu.AddMenuItem(new MenuItemViewModel("Disable 'Don't stop the music'", MaterialIconKind.Infinity, new RelayCommand(() => _playersService.PlayerDontStopTheMusic(p, false))));

                menu.AddMenuItem(items);
            }
#pragma warning restore CS4014
            //deep links don't seem to work  new MenuItemViewModel("Open settings", MaterialIconKind.Cog,  new RelayCommand(() => App.Launcher.LaunchUriAsync(new System.Uri("http://10.0.1.20:8095/#/settings/editplayer/sendspin-windows-magni")))),
            //new MenuItemViewModel("Open DSP settings", MaterialIconKind.Equaliser, null),

            
            MessageBus.Current.SendMessage<IPopupViewModel>(menu);
        });


    }

    private double _pendingVolume;
    private System.Timers.Timer? _volumeDebounceTimer;
    [GenerateTypedAction]
    public void VolumeChanged(object sender, object parameter)
    {
        //Debouncing inside itself so it doesnt' get into a loop fighting with MA sending back the new volume
        if (parameter is RangeBaseValueChangedEventArgs args)
        {
            
            var player = ((Slider)sender).DataContext as Player;

            if (args.NewValue == args.OldValue)
                return;
            var pendingVolume = args.NewValue;
            
            var _pendingVolume = args.NewValue;

            _volumeDebounceTimer?.Stop();
            _volumeDebounceTimer?.Dispose();

            _volumeDebounceTimer = new System.Timers.Timer(200);
            _volumeDebounceTimer.AutoReset = false;
            _volumeDebounceTimer.Elapsed += (s, e) =>
            {
#pragma warning disable CS4014
            _playersService.PlayerVolume((int)_pendingVolume, player);
#pragma warning restore CS4014
            };
            _volumeDebounceTimer.Start();
        }
    }

}