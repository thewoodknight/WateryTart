using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using WateryTart.Extensions;
using WateryTart.MassClient;
using WateryTart.MassClient.Events;
using WateryTart.MassClient.Messages;
using WateryTart.MassClient.Models;
using WateryTart.Settings;
using WateryTart.ViewModels;

namespace WateryTart.Services;

public partial class PlayersService : ReactiveObject, IPlayersService
{
    private readonly IMassWsClient _massClient;
    private readonly ISettings _settings;

    [Reactive] public partial ObservableCollection<PlayerViewModel> Players2 { get; set; }
    [Reactive] public partial ObservableCollection<Player> Players { get; set; }
    [Reactive] public partial ObservableCollection<PlayerQueue> Queues { get; set; }

    public Player SelectedPlayer
    {
        get => field;
        set
        {
            if (value != null)
            {
                _settings.LastSelectedPlayerId = value.PlayerId;
                Debug.WriteLine(value.PlaybackState.ToString());
            }

            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    public PlayersService(IMassWsClient massClient, ISettings settings)
    {
        _massClient = massClient;
        _settings = settings;

        Players = new ObservableCollection<Player>();
        Players2 = new ObservableCollection<PlayerViewModel>();
        Queues = new ObservableCollection<PlayerQueue>();

        _massClient.Events
            .Where(e => e is PlayerEventResponse)
            .Subscribe((e) => OnPlayerEvents((PlayerEventResponse)e));

        _massClient.Events
            .Where(e => e is PlayerQueueEventResponse)
            .Subscribe((e) => OnPlayerQueueEvents((PlayerQueueEventResponse)e));


    }

    public void OnPlayerQueueEvents(PlayerQueueEventResponse e)
    {

    }

    public void OnPlayerEvents(PlayerEventResponse e)
    {
        Debug.WriteLine(e.EventName);

        switch (e.EventName)
        {
            case EventType.PlayerAdded:
                if (!Players.Contains((e.data))) //Any?
                    Players.Add(e.data);

                break;
            case EventType.PlayerUpdated:
                if (e.data.Available == false)
                {
                    Players.RemoveAll(p => p.PlayerId == e.data.PlayerId);
                    break;
                }

                var player = Players.FirstOrDefault(p => p.PlayerId == e.data.PlayerId);
                if (player != null)
                player.PlaybackState = e.data.PlaybackState;

                break;
            case EventType.PlayerRemoved:
                Players.RemoveAll(p => p.PlayerId == e.data.PlayerId);
                break;
            default:

                break;
        }
    }

    public void GetPlayers()
    {
        _massClient
            .PlayersAll((a) =>
            {
                foreach (var y in a.Result)
                {
                    Players2.Add(new PlayerViewModel(y));
                    Players.Add(y);
                }

                if (!string.IsNullOrEmpty(_settings.LastSelectedPlayerId))
                {
                    SelectedPlayer =
                        Players.SingleOrDefault(player => player.PlayerId == _settings.LastSelectedPlayerId);
                }
            });

        _massClient.PlayerQueuesAll(a =>
        {
            foreach (var y in a.Result)
            {
                Queues.Add(y);
                Debug.WriteLine(y.display_name);
            }
        });
    }

    public void PlayerVolumeDown(Player p)
    {
    }

    public void PlayerVolumeUp(Player p)
    {
    }

    public void PlayerPlayPause(Player p)
    {

    }
    public void PlayItem(MediaItemBase t, Player? p = null, PlayerQueue? q = null, PlayMode mode = PlayMode.Play)
    {
        p ??= SelectedPlayer;

        _massClient.PlayerActiveQueue(p.PlayerId, (pq) =>
        {
            _massClient.Play(pq.Result.queue_id, t, mode, (a) =>
            {
                Debug.WriteLine(a);
            });
        });

    }
}