using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WateryTart.Core.Extensions;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Events;
using WateryTart.Service.MassClient.Models;
using MenuItemViewModel = WateryTart.Core.ViewModels.Menus.MenuItemViewModel;

namespace WateryTart.Core.Services;

public partial class PlayersService : ReactiveObject, IPlayersService, IAsyncReaper
{
    private readonly IMassWsClient _massClient;
    private readonly ISettings _settings;
    private readonly IColourService colourService;
    private ReadOnlyObservableCollection<QueuedItem> currentQueue;
    private ReadOnlyObservableCollection<QueuedItem> playedQueue;
    private CompositeDisposable _subscriptions = new CompositeDisposable();

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
                _ = FetchPlayerQueueAsync(value.PlayerId);
            }

            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    private async Task FetchPlayerQueueAsync(string id)
    {
        var pq = await _massClient.PlayerActiveQueueAsync(id);
        SelectedPlayerQueueId = pq.Result.queue_id;
        SelectedQueue = pq.Result;
        await FetchQueueContentsAsync();
    }

    private async Task FetchQueueContentsAsync()
    {
        var items = await _massClient.PlayerQueueItemsAsync(SelectedPlayerQueueId);
        QueuedItems.Clear();
        try
        {
            QueuedItems.AddRange(items.Result);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public PlayersService(IMassWsClient massClient, ISettings settings, IColourService colourService)
    {
        _massClient = massClient;
        _settings = settings;
        this.colourService = colourService;

        /* Subscribe to the relevant websocket events from MASS */
        _subscriptions.Add(_massClient.Events
            .Where(e => e is PlayerEventResponse)
            .Subscribe((e) => OnPlayerEvents((PlayerEventResponse)e)));

        _subscriptions.Add(_massClient.Events
            .Where(e => e is PlayerQueueEventResponse)
            .Subscribe((e) => OnPlayerQueueEvents((PlayerQueueEventResponse)e)));

        /* This takes care of filtering the two lists, though unsure on INPC */
        _subscriptions.Add(QueuedItems
                .Connect()
                .Sort(SortExpressionComparer<QueuedItem>.Ascending(t => t.sort_index))
                .Filter(i => i.sort_index > SelectedQueue.current_index)  //There is a "race condition" when new tracks are prepended to a queue I think
                .Bind(out currentQueue)
                .Subscribe());

        _subscriptions.Add(QueuedItems
                .Connect()
                .Sort(SortExpressionComparer<QueuedItem>.Descending(t => t.sort_index))
                .Filter(i => i.sort_index <= SelectedQueue.current_index)
                .Bind(out playedQueue)
                .Subscribe());

    }
    public async void OnPlayerQueueEvents(PlayerQueueEventResponse e)
    {

        switch (e.EventName)
        {
            case EventType.QueueAdded:
                break;

            case EventType.QueueUpdated: //replacing a queue is just 'updated'

                //It seems like when a queue is updated, the best thing is to clear/refetch
                if (SelectedQueue != null)
                {
                    SelectedQueue.current_index = e.data.current_index;
                    SelectedQueue.current_item = e.data.current_item;

                    var currentItem = SelectedQueue.current_item;
                    if (currentItem == null)
                        break;
                    if (currentItem.image.remotely_accessible)
                        colourService.Update(currentItem.media_item.ItemId, currentItem.image.path);
                    else
                    {
                        var url = string.Format("http://{0}/imageproxy?path={1}&provider={2}&checksum=&size=256", App.BaseUrl, Uri.EscapeDataString(currentItem.image.path), currentItem.image.provider);
                        colourService.Update(currentItem.media_item.ItemId, url);
                    }
                }

                await FetchQueueContentsAsync();
                break;

            case EventType.QueueItemsUpdated:

                break;

            default:
                break;
        }
    }

    public void OnPlayerEvents(PlayerEventResponse e)
    {
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

    public async void GetPlayers()
    {
        try
        {
            var playersResponse = await _massClient.PlayersAllAsync();
            foreach (var y in playersResponse.Result)
            {
                Players.Add(y);
            }

            var queuesResponse = await _massClient.PlayerQueuesAllAsync();
            foreach (var y in queuesResponse.Result)
            {
                Queues.Add(y);

            }

            if (!string.IsNullOrEmpty(_settings.LastSelectedPlayerId))
            {
                SelectedPlayer =
                    Players.SingleOrDefault(player => player.PlayerId == _settings.LastSelectedPlayerId);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading players: {ex.Message}");
        }
    }

    public async void PlayerVolumeDown(Player p)
    {
        p ??= SelectedPlayer;

        if (p != null)
        {
            try
            {
                await _massClient.PlayerGroupVolumeDownAsync(p.PlayerId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adjusting volume: {ex.Message}");
            }
        }
    }

    public async void PlayerVolumeUp(Player p)
    {
        p ??= SelectedPlayer;

        if (p != null)
        {
            try
            {
                await _massClient.PlayerGroupVolumeUpAsync(p.PlayerId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adjusting volume: {ex.Message}");
            }
        }
    }

    public async void PlayerPlayPause(Player p)
    {
        p ??= SelectedPlayer;
        try
        {
            await _massClient.PlayerPlayPauseAsync(p.PlayerId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error toggling playback: {ex.Message}");
        }
    }

    public async void PlayItem(MediaItemBase t, Player? p = null, PlayerQueue? q = null, PlayMode mode = PlayMode.Replace)
    {
        p ??= SelectedPlayer;

        if (p == null)
        {
            //This really shouldn't be in the service, but shhhh
            var menu = new MenuViewModel();
            foreach (var player in Players)
            {
                var capturedPlayer = player;
                menu.AddMenuItem(new MenuItemViewModel($"\tPlay on {player.DisplayName}", string.Empty, ReactiveCommand.Create<Unit>(async _ =>
                {
                    try
                    {
                        SelectedPlayer = capturedPlayer;
                        var pq = await _massClient.PlayerActiveQueueAsync(capturedPlayer.PlayerId);
                        await _massClient.PlayAsync(pq.Result.queue_id, t, mode);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error playing item: {ex.Message}");
                    }
                })));
            }
            MessageBus.Current.SendMessage(menu);
        }
        else
        {
            try
            {
                var pq = await _massClient.PlayerActiveQueueAsync(p.PlayerId);
                await _massClient.PlayAsync(pq.Result.queue_id, t, mode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing item: {ex.Message}");
            }
        }
    }

    public async void PlayerNext(Player p = null)
    {
        p ??= SelectedPlayer;
        try
        {
            await _massClient.PlayerNextAsync(p.PlayerId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error playing next: {ex.Message}");
        }
    }

    public async void PlayerPrevious(Player p)
    {
        p ??= SelectedPlayer;
        try
        {
            await _massClient.PlayerPreviousAsync(p.PlayerId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error playing previous: {ex.Message}");
        }
    }

    public async Task ReapAsync()
    {
        QueuedItems?.Dispose();
        _subscriptions?.Dispose();
        try
        {
            await _massClient.DisconnectAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during disconnect: {ex}");
        }
    }

    public void Reap()
    {
        QueuedItems?.Dispose();
        _subscriptions?.Dispose();

        try
        {
            // Synchronously wait for the disconnect to complete
            var disconnectTask = _massClient.DisconnectAsync();
            disconnectTask.Wait(TimeSpan.FromSeconds(10));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during disconnect: {ex}");
        }
    }
}