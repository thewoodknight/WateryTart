using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WateryTart.Core.Extensions;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Events;
using WateryTart.Service.MassClient.Models;
using static Microsoft.IO.RecyclableMemoryStreamManager;

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

    [Reactive] public partial double Progress { get; set; }
    public Player SelectedPlayer
    {
        get => field;
        set
        {
            if (value != null)
            {
                _settings.LastSelectedPlayerId = value.PlayerId;
#pragma warning disable CS4014 // Fire-and-forget intentional - fetches queue in background
                _ = FetchPlayerQueueAsync(value.PlayerId);
#pragma warning restore CS4014
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
            if (items.Result != null)
                QueuedItems.AddRange(items.Result);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private DispatcherTimer _timer;

    public PlayersService(IMassWsClient massClient, ISettings settings, IColourService colourService)
    {
        _massClient = massClient;
        _settings = settings;
        this.colourService = colourService;

        /* Subscribe to the relevant websocket events from MASS */
        _subscriptions.Add(_massClient.Events
            .ObserveOn(RxApp.MainThreadScheduler)
            .SelectMany(e => Observable.FromAsync(() => OnEvents(e)))
            .Subscribe());


        /* This takes care of filtering the two lists, though unsure on INPC */
        _subscriptions.Add(QueuedItems
                .Connect()
                .Sort(SortExpressionComparer<QueuedItem>.Ascending(t => t.sort_index))
                .Filter(i => SelectedQueue != null && i.sort_index > SelectedQueue.current_index)  //There is a "race condition" when new tracks are prepended to a queue I think
                .Bind(out currentQueue)
                .Subscribe());

        _subscriptions.Add(QueuedItems
                .Connect()
                .Sort(SortExpressionComparer<QueuedItem>.Descending(t => t.sort_index))
                .Filter(i => SelectedQueue != null && i.sort_index <= SelectedQueue.current_index)
                .Bind(out playedQueue)
                .Subscribe());

        _timer = new DispatcherTimer();
        _timer.Interval = new TimeSpan(0, 0, 1);
        _timer.Tick += T_Tick;
        _timer.Start();
    }

    private void T_Tick(object? sender, EventArgs e)
    {
        if (SelectedPlayer?.PlaybackState != PlaybackState.playing)
            return;

        if (SelectedQueue?.current_item?.media_item != null)
        {
            Progress = SelectedQueue.current_item.media_item.progress;
            SelectedQueue.current_item.media_item.elapsed_time += 1;
        }
    }

    public async Task OnEvents(BaseEventResponse e)
    {
        PlayerEventResponse playerEvent;
        PlayerQueueEventResponse queueEvent;
        PlayerQueueTimeUpdatedEventResponse timeEvent;
        MediaItemEventResponse mediaEvent;
        switch (e.EventName)
        {
            case EventType.MediaItemPlayed:
                mediaEvent = (MediaItemEventResponse)e;
                if (mediaEvent.object_id == SelectedQueue?.current_item?.media_item.Uri)
                    SelectedQueue.current_item.media_item.elapsed_time = mediaEvent.data.seconds_played;
                break;
            case EventType.QueueTimeUpdated:
                timeEvent = (PlayerQueueTimeUpdatedEventResponse)e;
                if (SelectedQueue != null && e.object_id == SelectedQueue.queue_id)
                {
                    SelectedQueue.current_item.media_item.elapsed_time = timeEvent.data;
                }
                break;

            case EventType.PlayerAdded:
                playerEvent = (PlayerEventResponse)e;
                if (!Players.Contains((playerEvent.data)) && playerEvent.data != null) //Any?
                    Players.Add(playerEvent.data);

                break;

            case EventType.PlayerUpdated:
                playerEvent = (PlayerEventResponse)e;
                var player = Players.FirstOrDefault(p => p.PlayerId == playerEvent.data.PlayerId);

                if (player != null)
                {
                    player.PlaybackState = playerEvent.data.PlaybackState;
                    player.CurrentMedia = playerEvent.data.CurrentMedia; // this should probably be more of a clone
                    player.VolumeLevel = playerEvent.data.VolumeLevel;

                }
                break;

            case EventType.PlayerRemoved:
                playerEvent = (PlayerEventResponse)e;
                Players.RemoveAll(p => p.PlayerId == playerEvent.data.PlayerId);
                break;

            case EventType.QueueAdded:
                break;

            case EventType.QueueUpdated: //replacing a queue is just 'updated'

                queueEvent = (PlayerQueueEventResponse)e;
                //It seems like when a queue is updated, the best thing is to clear/refetch
                if (SelectedQueue != null && queueEvent.data.queue_id == SelectedQueue.queue_id)
                {
                    SelectedQueue.current_index = queueEvent.data.current_index;
                    SelectedQueue.current_item = queueEvent.data.current_item;

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

            default:
                break;
        }
    }
    public async Task GetPlayers()
    {
        try
        {
            var playersResponse = await _massClient.PlayersAllAsync();
            foreach (var y in playersResponse.Result)
            {
                if (y != null)
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

    public async Task PlayerVolumeDown(Player p)
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

    public async Task PlayerVolumeUp(Player p)
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

    public async Task PlayerPlayPause(Player p)
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

    public async Task PlayItem(MediaItemBase t, Player? p = null, PlayerQueue? q = null, PlayMode mode = PlayMode.Replace, bool RadioMode = false)
    {
        p ??= SelectedPlayer;

        if (p == null)
        {
            var menu = MenuHelper.AddPlayers(this, t);
            MessageBus.Current.SendMessage(menu);
        }
        else
        {
            try
            {
                var pq = await _massClient.PlayerActiveQueueAsync(p.PlayerId);
                await _massClient.PlayAsync(pq.Result.queue_id, t, mode, RadioMode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing item: {ex.Message}");
            }
        }
    }

    public async Task PlayerNext(Player p = null)
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

    public async Task PlayerPrevious(Player p)
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

    public async Task PlayArtistRadio(Artist artist)
    {
        PlayItem(artist, RadioMode: true);
    }

    public async Task ReapAsync()
    {
        _timer?.Stop();
        _timer = null;
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
        _timer?.Stop();
        _timer = null;
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