using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WateryTart.Core.Converters;
using WateryTart.Core.Extensions;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.ViewModels.Popups;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Events;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.Services;

public partial class PlayersService : ReactiveObject, IAsyncReaper
{
    private readonly MusicAssistantClient _massClient;
    private readonly ISettings _settings;
    private readonly ColourService _colourService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<PlayersService> _logger;
    private ReadOnlyObservableCollection<TrackViewModel> currentQueue;
    private ReadOnlyObservableCollection<TrackViewModel> playedQueue;
    private CompositeDisposable _subscriptions = new CompositeDisposable();
    private readonly Dictionary<string, TrackViewModel> _trackViewModelCache = new Dictionary<string, TrackViewModel>();
    private bool _isFetchingQueueContents = false;
    private DispatcherTimer _timer;

    [Reactive] public partial SourceCache<QueuedItem, string> QueuedItems { get; set; } = new SourceCache<QueuedItem, string>(x => x.QueueItemId);
    [Reactive] public partial ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>();
    [Reactive] public partial ObservableCollection<PlayerQueue> Queues { get; set; } = new ObservableCollection<PlayerQueue>();
    [Reactive] public partial string SelectedPlayerQueueId { get; set; } = string.Empty;
    [Reactive] public partial PlayerQueue? SelectedQueue { get; set; } = new PlayerQueue();
    public ReadOnlyObservableCollection<TrackViewModel> CurrentQueue { get => currentQueue; }
    public ReadOnlyObservableCollection<TrackViewModel> PlayedQueue { get => playedQueue; }

    [Reactive] public partial double Progress { get; set; }

    public Player? SelectedPlayer
    {
        get => field;
        set
        {
            if (value != null)
            {
                _settings.LastSelectedPlayerId = value.PlayerId;
#pragma warning disable CS4014
                _ = FetchPlayerQueueAsync(value.PlayerId);
#pragma warning restore CS4014
            }

            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    private async Task FetchPlayerQueueAsync(string id)
    {
        var pq = await _massClient.WithWs().GetPlayerActiveQueueAsync(id);
        if (pq?.Result == null)
            return;

        SelectedPlayerQueueId = pq.Result.QueueId;
        SelectedQueue = pq.Result;
        FetchQueueContentsAsync();
    }

    private async Task FetchQueueContentsAsync()
    {
        if (_isFetchingQueueContents)
            return;

        var items = await _massClient.WithWs().GetPlayerQueueItemsAsync(SelectedPlayerQueueId);
        try
        {
            if (items.Result != null)
            {
                // Build dictionaries for fast lookup
                var newItemsDict = items.Result.ToDictionary(i => i.QueueItemId);
                var currentItemsDict = QueuedItems.Items.ToDictionary(i => i.QueueItemId);

                // Remove items not in the new result
                var toRemove = QueuedItems.Items.Where(i => !newItemsDict.ContainsKey(i.QueueItemId)).ToList();
                foreach (var item in toRemove)
                    QueuedItems.Remove(item);

                // Update existing items
                foreach (var item in QueuedItems.Items)
                {
                    if (newItemsDict.TryGetValue(item.QueueItemId, out var newItem))
                    {
                        // Update properties as needed
                        item.SortIndex = newItem.SortIndex;
                        item.MediaItem = newItem.MediaItem;
                        // Add more property updates if needed
                    }
                }

                // Add new items not already present
                var currentIds = currentItemsDict.Keys;
                var toAdd = items.Result.Where(i => !currentIds.Contains(i.QueueItemId)).ToList();
                if (toAdd.Count > 0)
                    foreach (var item in toAdd)
                        QueuedItems.AddOrUpdate(item);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching queue contents");
        }
        finally
        {
            _isFetchingQueueContents = false;
        }
    }



    public PlayersService(MusicAssistantClient massClient, ISettings settings, ColourService colourService, ILoggerFactory loggerFactory)
    {
        _massClient = massClient;
        _settings = settings;
        _colourService = colourService;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<PlayersService>();

        /* Subscribe to the relevant websocket events from MASS */
        _subscriptions.Add(_massClient.WithWs().Events
            .ObserveOn(RxApp.MainThreadScheduler)
            .SelectMany(e => Observable.FromAsync(() => OnEvents(e)))
            .Subscribe());

        /* This takes care of filtering the two lists */
        _subscriptions.Add(QueuedItems
                .Connect()
                .AutoRefresh(x => x.SortIndex)
                .AutoRefresh(x => x.MediaItem)
                .Sort(SortExpressionComparer<QueuedItem>.Ascending(t => t.SortIndex))
                .Filter(i => SelectedQueue != null && (i.SortIndex > SelectedQueue.CurrentIndex || i.MediaItem?.ItemId == SelectedQueue.CurrentItem?.MediaItem?.ItemId))
                .Transform(i => GetOrCreateTrackViewModel(i))   //transform is expensive since the list is reset to 0, and despite caching still means the visual tree is recreated
                .Bind(out currentQueue)
                .Subscribe());


        _subscriptions.Add(QueuedItems
                .Connect()
                .AutoRefresh(x => x.SortIndex)
                .AutoRefresh(x => x.MediaItem)
                .Sort(SortExpressionComparer<QueuedItem>.Descending(t => t.SortIndex))
                .Filter(i => SelectedQueue != null && i.SortIndex < SelectedQueue.CurrentIndex)
                .Transform(i => GetOrCreateTrackViewModel(i))
                .Bind(out playedQueue)
                .Subscribe());

        this.WhenAnyValue(x => x.SelectedQueue.CurrentIndex)
            .Where(_ => SelectedQueue != null)
            .Subscribe(_ => QueuedItems.Refresh());

        _timer = new DispatcherTimer();
        _timer.Interval = new TimeSpan(0, 0, 1);
        _timer.Tick += T_Tick;
        _timer.Start();
    }

    private void T_Tick(object? sender, EventArgs e)
    {
        if (SelectedPlayer?.PlaybackState != PlaybackState.Playing)
            return;

        if (SelectedQueue?.CurrentItem?.MediaItem != null)
        {
            Progress = SelectedQueue.CurrentItem.MediaItem.Progress;
            SelectedQueue.CurrentItem.MediaItem.ElapsedTime += 1;
        }
    }

    public async Task OnEvents(BaseEventResponse e)
    {
        PlayerEventResponse playerEvent;
        PlayerQueueEventResponse queueEvent;
        PlayerQueueTimeUpdatedEventResponse timeEvent;
        MediaItemEventResponse mediaEvent;
        MediaItemEvent2Response mediaEvent2;
        Debug.WriteLine(e.EventName);
        switch (e.EventName)
        {
            case EventType.MediaItemUpdated:
                mediaEvent = (MediaItemEventResponse)e;
                var item = QueuedItems.Items.FirstOrDefault(i => i.MediaItem?.Uri == mediaEvent.data.Uri);
                if (item != null)
                {
                    item.MediaItem.Favorite = mediaEvent.data.Favorite;
                    //QueuedItems.Refresh(item);
                }

                if (SelectedQueue?.CurrentItem?.MediaItem?.Uri == mediaEvent?.data?.Uri)
                {
                    SelectedQueue.CurrentItem.MediaItem.Favorite = mediaEvent.data.Favorite;
                }

                break;
            case EventType.MediaItemPlayed:
                mediaEvent2 = (MediaItemEvent2Response)e;
                if (mediaEvent2.object_id == SelectedQueue?.CurrentItem?.MediaItem?.Uri)
                    if (mediaEvent2.data.SecondsPlayed != null)
                        SelectedQueue?.CurrentItem?.MediaItem?.ElapsedTime = mediaEvent2.data.SecondsPlayed.Value;
                break;

            case EventType.QueueTimeUpdated:
                timeEvent = (PlayerQueueTimeUpdatedEventResponse)e;
                if (SelectedQueue != null && e.object_id == SelectedQueue.QueueId)
                {
                    SelectedQueue.CurrentItem.MediaItem.ElapsedTime = timeEvent.data;
                }
                break;

            case EventType.PlayerAdded:
                playerEvent = (PlayerEventResponse)e;
                if (!Players.Contains((playerEvent.data)) && playerEvent.data != null)
                    Players.Add(playerEvent.data);
                break;

            case EventType.PlayerUpdated:
                playerEvent = (PlayerEventResponse)e;
                var player = Players.FirstOrDefault(p => p.PlayerId == playerEvent.data.PlayerId);

                if (player != null)
                {
                    player.PlaybackState = playerEvent.data.PlaybackState;
                    player.CurrentMedia = playerEvent.data.CurrentMedia;
                    player.VolumeLevel = playerEvent.data.VolumeLevel;
                }
                break;

            case EventType.PlayerRemoved:
                playerEvent = (PlayerEventResponse)e;
                Players.RemoveAll(p => p.PlayerId == playerEvent.data.PlayerId);
                break;

            case EventType.QueueAdded:

                queueEvent = (PlayerQueueEventResponse)e;
                var existing = Queues.FirstOrDefault(q => q.QueueId == queueEvent.data.QueueId);
                if (existing == null)
                    Queues.Add(queueEvent.data);
                else
                    Queues.ReplaceOrAdd(existing, queueEvent.data);
                break;

            case EventType.QueueUpdated:
                queueEvent = (PlayerQueueEventResponse)e;
                if (SelectedQueue != null && queueEvent?.data?.QueueId == SelectedQueue.QueueId)
                {
                    SelectedQueue.CurrentIndex = queueEvent.data.CurrentIndex;
                    SelectedQueue.CurrentItem = queueEvent.data.CurrentItem;
                    var currentItem = SelectedQueue.CurrentItem;
                    if (currentItem == null)
                        break;

                    //Get the new background colours
                    if (currentItem.Image != null && currentItem.Image.RemotelyAccessible)
                        _ = _colourService.Update(currentItem.MediaItem.ItemId, currentItem.Image.Path);
                    else
                    {
                        var url = ImagePathHelper.ProxyString(currentItem.Image.Path, currentItem.Image.Provider);
                        _ = _colourService.Update(currentItem.MediaItem.ItemId, url);
                    }
                }
                break;
            case EventType.QueueItemsUpdated:
                FetchQueueContentsAsync();
                break;
            default:
                break;
        }
    }

    public async Task GetPlayers()
    {
        try
        {
            var playersResponse = await _massClient.WithWs().GetPlayersAllAsync();
            foreach (var y in playersResponse.Result)
            {
                if (y != null)
                    Players.Add(y);
            }

            var queuesResponse = await _massClient.WithWs().GetPlayerQueuesAllAsync();
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
            _logger.LogError(ex, "Error loading players");
        }
    }

    public async Task PlayerVolume(int volume, Player? p = null)
    {
        p ??= SelectedPlayer;

        if (p != null)
        {
            try
            {
                var result = await _massClient.WithWs().SetPlayerGroupVolumeAsync(p.PlayerId, volume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting volume down for player {PlayerId}", p.PlayerId);
            }
        }
    }

    public async Task PlayerVolumeDown(Player? p = null)
    {
        p ??= SelectedPlayer;

        if (p != null)
        {
            try
            {
                await _massClient.WithWs().PlayerGroupVolumeDownAsync(p.PlayerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting volume down for player {PlayerId}", p.PlayerId);
            }
        }
    }

    public async Task PlayerVolumeUp(Player p = null)
    {
        p ??= SelectedPlayer;

        if (p != null)
        {
            try
            {
                await _massClient.WithWs().PlayerGroupVolumeUpAsync(p.PlayerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting volume up for player {PlayerId}", p.PlayerId);
            }
        }
    }

    public async Task PlayerPlayPause(Player? p  = null)
    {
        p ??= SelectedPlayer;
        try
        {
            await _massClient.WithWs().PlayerPlayPauseAsync(p.PlayerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling playback for player {PlayerId}", p?.PlayerId);
        }
    }

    public async Task PlayItem(MediaItemBase t, Player? p = null, PlayerQueue? q = null, PlayMode mode = PlayMode.Replace, bool RadioMode = false)
    {
        p ??= SelectedPlayer;

        if (p == null)
        {
            _logger.LogInformation("No player was selected, present popup to pick");
            var players = MenuHelper.AddPlayers(this, t);
            var menu = new MenuViewModel(players);
            MessageBus.Current.SendMessage<IPopupViewModel>(menu);
        }
        else
        {
            try
            {
                _logger.LogInformation($"Playing {t.Name}");
                var pq = await _massClient.WithWs().GetPlayerActiveQueueAsync(p.PlayerId);
                await _massClient.WithWs().PlayAsync(pq.Result.QueueId, t, mode, RadioMode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error playing item {ItemName} on player {PlayerId}", t?.Name, p?.PlayerId);
            }
        }
    }

    public async Task PlayerNext(Player? p = null)
    {
        p ??= SelectedPlayer;
        if (p?.PlayerId == null)
            return;
        try
        {
#pragma warning disable CS4014
            _ = _massClient.WithWs().PlayerNextAsync(p.PlayerId);
#pragma warning restore CS4014
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing next on player {PlayerId}", p.PlayerId);
        }
    }

    public async Task PlayerSeek(Player? p, int position)
    {
        p ??= SelectedPlayer;
        if (p?.PlayerId == null)
            return;
        try
        {
#pragma warning disable CS4014
            _ = _massClient.WithWs().PlayerSeekAsync(p.PlayerId, position);
#pragma warning restore CS4014
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing previous on player {PlayerId}", p?.PlayerId);
        }
    }

    public async Task PlayerPrevious(Player? p = null)
    {
        p ??= SelectedPlayer;
        if (p?.PlayerId == null)
            return;
        try
        {
#pragma warning disable CS4014
            _ = _massClient.WithWs().PlayerPreviousAsync(p.PlayerId);
#pragma warning restore CS4014
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing previous on player {PlayerId}", p?.PlayerId);
        }
    }

    public async Task PlayArtistRadio(Artist artist)
    {
        _ = PlayItem(artist, RadioMode: true);
    }

    public async Task ReapAsync()
    {
        _timer?.Stop();
        QueuedItems?.Dispose();
        _subscriptions?.Dispose();

        // Clean up cached ViewModels
        foreach (var vm in _trackViewModelCache.Values)
        {
            vm.Dispose();
        }
        _trackViewModelCache.Clear();

        try
        {
            await _massClient.WithWs().DisconnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect");
        }
    }

    public void Reap()
    {
        _timer?.Stop();
        _timer = null;
        QueuedItems?.Dispose();
        _subscriptions?.Dispose();

        // Clean up cached ViewModels
        foreach (var vm in _trackViewModelCache.Values)
        {
            vm.Dispose();
        }
        _trackViewModelCache.Clear();

        try
        {
            var disconnectTask = _massClient.WithWs().DisconnectAsync();
            disconnectTask.Wait(TimeSpan.FromSeconds(10));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnect");
        }
    }

    private TrackViewModel GetOrCreateTrackViewModel(QueuedItem? queuedItem)
    {
        if (queuedItem == null)
            return null;

        var cacheKey = queuedItem.QueueItemId;

        if (_trackViewModelCache.TryGetValue(cacheKey, out var cachedViewModel))
        {
            return cachedViewModel;
        }

        var trackViewModel = new TrackViewModel(_massClient, null, this, queuedItem.MediaItem);
        _trackViewModelCache[cacheKey] = trackViewModel;
        return trackViewModel;
    }

    public async Task PlayerRemoveFromFavorites(MediaItemBase item)
    {
        if (item == null) 
            return;
        await _massClient.WithWs().RemoveFavoriteItemAsync(item);
    }

    public async Task PlayerAddToFavorites(MediaItemBase item)
    {
        if (item == null) 
            return;
        await _massClient.WithWs().AddFavoriteItemAsync(item);
    }


    public async Task PlayerSetRepeatMode(RepeatMode repeatmode, Player? p = null)
    {
        if (p== null)
            p = SelectedPlayer; 

        await _massClient.WithWs().SetPlayerQueueRepeatAsync(p.ActiveSource, repeatmode);
    }

    public async Task PlayerShuffle(Player? p = null, bool shuffle = true)
    {
        p ??= SelectedPlayer;
        if (p?.PlayerId == null)
            return;
        try
        {
#pragma warning disable CS4014
            _ = _massClient.WithWs().SetPlayerQueueShuffleAsync(p.ActiveSource, shuffle);
#pragma warning restore CS4014
        }
        catch (Exception ex)
        {
            
        }
    }
    public async Task PlayerDontStopTheMusic(Player? p = null, bool dontstop = true)
    {
        p ??= SelectedPlayer;
        if (p?.PlayerId == null)
            return;
        try
        {
#pragma warning disable CS4014
            _ = _massClient.WithWs().SetPlayerQueueDontStopTheMusicAsync(p.ActiveSource, dontstop);
#pragma warning restore CS4014
        }
        catch (Exception ex)
        {
            
        }
    }

    public async Task PlayerClearQueue(Player? p = null)
    {
        p ??= SelectedPlayer;
        if (p?.PlayerId == null)
            return;
        try
        {
#pragma warning disable CS4014
            _ = _massClient.WithWs().ClearPlayerQueueAsync(p.ActiveSource);
#pragma warning restore CS4014
        }
        catch (Exception ex)
        {

        }
    }
}