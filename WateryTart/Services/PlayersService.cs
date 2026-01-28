using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using WateryTart.Extensions;
using WateryTart.MassClient;
using WateryTart.MassClient.Events;
using WateryTart.MassClient.Messages;
using WateryTart.MassClient.Models;
using WateryTart.Settings;
using WateryTart.ViewModels;
using WateryTart.ViewModels.Menus;

namespace WateryTart.Services;

public partial class PlayersService : ReactiveObject, IPlayersService
{
    private readonly IMassWsClient _massClient;
    private readonly ISettings _settings;
    private ReadOnlyObservableCollection<QueuedItem> currentQueue;
    private ReadOnlyObservableCollection<QueuedItem> playedQueue;

    [Reactive] public partial SourceList<QueuedItem> QueuedItems { get; set; } = new SourceList<QueuedItem>();
    [Reactive] public partial ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>();
    [Reactive] public partial ObservableCollection<PlayerQueue> Queues { get; set; } = new ObservableCollection<PlayerQueue>();
    [Reactive] public partial string SelectedPlayerQueueId { get; set; }
    [Reactive] public partial PlayerQueue SelectedQueue { get; set; }
    public ReadOnlyObservableCollection<QueuedItem> CurrentQueue { get => currentQueue; }
    public ReadOnlyObservableCollection<QueuedItem> PlayedQueue { get => playedQueue; }

    public Player SelectedPlayer
    {
        get => field;
        set
        {
            if (value != null)
            {
                _settings.LastSelectedPlayerId = value.PlayerId;
                FetchPlayerQueue(value.PlayerId);
            }

            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    private void FetchPlayerQueue(string id)
    {
        _massClient.PlayerActiveQueue(id, (pq) =>
        {
            SelectedPlayerQueueId = pq.Result.queue_id;
            SelectedQueue = pq.Result;
            FetchQueueContents();
        });
    }

    private void FetchQueueContents()
    {
        _massClient.PlayerQueueItems(SelectedPlayerQueueId, (items) =>
        {
            QueuedItems.Clear();
            QueuedItems.AddRange(items.Result);
        });
    }

    public PlayersService(IMassWsClient massClient, ISettings settings)
    {
        _massClient = massClient;
        _settings = settings;

        /* Subscribe to the relevant websocket events from MASS */
        _massClient.Events
            .Where(e => e is PlayerEventResponse)
            .Subscribe((e) => OnPlayerEvents((PlayerEventResponse)e));

        _massClient.Events
            .Where(e => e is PlayerQueueEventResponse)
            .Subscribe((e) => OnPlayerQueueEvents((PlayerQueueEventResponse)e));

        /* This takes care of filtering the two lists, though unsure on INPC */
        QueuedItems
                .Connect()
                .Filter(i => i.sort_index > SelectedQueue.current_index)
                .Bind(out currentQueue)
                .Subscribe();

        QueuedItems
                .Connect()
                .Filter(i => i.sort_index <= SelectedQueue.current_index)
                .Bind(out playedQueue)
                .Subscribe();
    }

    public void OnPlayerQueueEvents(PlayerQueueEventResponse e)
    {
        Debug.WriteLine(e.EventName);
        switch (e.EventName)
        {
            case EventType.QueueAdded:

                break;

            case EventType.QueueUpdated: //replacing a queue is just 'updated'
                Debug.WriteLine($"{e.data.items} items now in the queue");
                //It seems like when a queue is updated, the best thing is to clear/refetch
                SelectedQueue.current_index = e.data.current_index;
                FetchQueueContents();
                break;

            case EventType.QueueItemsUpdated:
                Debug.WriteLine($"{e.data.items} items now in the queue");
                break;

            default:

                break;
        }
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
                var player = Players.FirstOrDefault(p => p.PlayerId == e.data.PlayerId);

                if (player != null)
                {
                    player.PlaybackState = e.data.PlaybackState;
                    player.CurrentMedia = e.data.CurrentMedia; // this should probably be more of a clone
                    player.VolumeLevel = e.data.VolumeLevel;
                }

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
        p ??= SelectedPlayer;
        _massClient.PlayerPlayPause(p.PlayerId, (a) => { });
    }

    public void PlayItem(MediaItemBase t, Player? p = null, PlayerQueue? q = null, PlayMode mode = PlayMode.Replace)
    {
        p ??= SelectedPlayer;

        if (p == null)
        {
            //This really shouldn't be in the service, but shhhh
            var menu = new MenuViewModel();
            foreach (var player in Players)
            {
                menu.AddMenuItem(new MenuItemViewModel($"\tPlay on {player.DisplayName}", string.Empty, ReactiveCommand.Create<Unit>(r =>
                {
                    _massClient.PlayerActiveQueue(player.PlayerId, (pq) =>
                    {
                        SelectedPlayer = player;
                        _massClient.Play(pq.Result.queue_id, t, mode, (a) =>
                        {
                            Debug.WriteLine(a);
                        });
                    });
                })));
            }
            MessageBus.Current.SendMessage(menu);
        }
        else
        {
            _massClient.PlayerActiveQueue(p.PlayerId, (pq) =>
            {
                _massClient.Play(pq.Result.queue_id, t, mode, (a) =>
                {
                    //Should events be raised here?
                });
            });
        }
    }

    public void PlayerNext(Player p = null)
    {
        p ??= SelectedPlayer;
        _massClient.PlayerNext(p.PlayerId, (a) => { });
    }

    public void PlayerPrevious(Player p)
    {
        p ??= SelectedPlayer;
        _massClient.PlayerPrevious(p.PlayerId, (a) => { });
    }
}