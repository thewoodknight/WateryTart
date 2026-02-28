using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
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
    private readonly ILogger<PlayersService> _logger;
    private readonly ReadOnlyObservableCollection<TrackViewModel> currentQueue;
    private readonly ReadOnlyObservableCollection<TrackViewModel> playedQueue;
    private readonly CompositeDisposable _subscriptions = [];
    private readonly Dictionary<string, TrackViewModel> _trackViewModelCache = [];
    private bool _isFetchingQueueContents = false;
    private DispatcherTimer _timer;

    // Volume coordination
    private readonly SemaphoreSlim _volumeSemaphore = new(1, 1);
    private readonly ConcurrentDictionary<string, (int Volume, DateTime Timestamp)> _pendingLocalVolumeChanges = new();
    private static readonly TimeSpan EchoIgnoreWindow = TimeSpan.FromMilliseconds(700);
    private const int VolumeTolerance = 1;

    [Reactive] public partial SourceCache<QueuedItem, string> QueuedItems { get; set; } = new SourceCache<QueuedItem, string>(x => x.QueueItemId!);
    [Reactive] public partial ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>();
    [Reactive] public partial ObservableCollection<PlayerQueue?> Queues { get; set; } = new ObservableCollection<PlayerQueue?>();
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
                _settings.LastSelectedPlayerId = value.PlayerId!;
                _ = FetchPlayerQueueAsync(value.PlayerId!);
            }

            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    private async Task FetchPlayerQueueAsync(string id)
    {
        var pq = await _massClient.WithWs().GetPlayerActiveQueueAsync(id);
        if (pq?.Result == null)
            return;

        SelectedPlayerQueueId = pq?.Result?.QueueId!;
        SelectedQueue = pq!.Result;
        await FetchQueueContentsAsync();
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
                var newItemsDict = items?.Result?.ToDictionary(i => i.QueueItemId!);
                var currentItemsDict = QueuedItems?.Items?.ToDictionary(i => i.QueueItemId!);

                // Remove items not in the new result
                var toRemove = QueuedItems?.Items.Where(i => !newItemsDict!.ContainsKey(i.QueueItemId!)).ToList();
                foreach (var item in toRemove!)
                    QueuedItems?.Remove(item);

                // Update existing items
                foreach (var item in QueuedItems?.Items!)
                {
                    if (newItemsDict == null)
                        continue;

                    if (newItemsDict.TryGetValue(item.QueueItemId!, out var newItem))
                    {
                        // Update properties as needed
                        item.SortIndex = newItem.SortIndex;
                        item.MediaItem = newItem.MediaItem;
                        // Add more property updates if needed
                    }
                }

                // Add new items not already present
                var currentIds = currentItemsDict?.Keys;
                if (currentIds != null)
                {
                    var toAdd = items?.Result.Where(i => !currentIds.Contains(i.QueueItemId!)).ToList();
                    if (toAdd?.Count > 0)
                        foreach (var item in toAdd)
                            QueuedItems.AddOrUpdate(item);
                }
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
        _logger = loggerFactory.CreateLogger<PlayersService>();

        /* Subscribe to the relevant websocket events from MASS */
        _subscriptions.Add(_massClient.WithWs().Events
            .ObserveOn(RxApp.MainThreadScheduler)
            .SelectMany(e => Observable.FromAsync(() => OnEvents(e)))
            .Subscribe());

        /* This takes care of filtering the two lists */
        // While .Sort is deprecated, the .Transform() gets in the way of .SortAndBind, since the list is transformed before sorting.
        // Neither Avalonia, ReactiveUI or DynamicData have examples for SortAndBind

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        _subscriptions.Add(QueuedItems
                .Connect()
                .AutoRefresh(x => x.SortIndex)
                .AutoRefresh(x => x.MediaItem)
                .Sort(SortExpressionComparer<QueuedItem>.Ascending(t => t.SortIndex))
                .Filter(i => SelectedQueue != null && (i.SortIndex > SelectedQueue.CurrentIndex || i.MediaItem?.ItemId == SelectedQueue.CurrentItem?.MediaItem?.ItemId))
                .Transform(i => GetOrCreateTrackViewModel(i)!)
                .Bind(out currentQueue)
                .Subscribe());



        _subscriptions.Add(QueuedItems
                .Connect()
                .AutoRefresh(x => x.SortIndex)
                .AutoRefresh(x => x.MediaItem)
                .Sort(SortExpressionComparer<QueuedItem>.Descending(t => t.SortIndex))
                .Filter(i => SelectedQueue != null && i.SortIndex < SelectedQueue.CurrentIndex)
                .Transform(i => GetOrCreateTrackViewModel(i)!)
                .Bind(out playedQueue)
                .Subscribe());
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
#pragma warning restore CS0618 // Type or member is obsolete

        this.WhenAnyValue(x => x.SelectedQueue!.CurrentIndex)
            .Where(_ => SelectedQueue != null)
            .Subscribe(_ => QueuedItems.Refresh());

        _timer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 1)
        };
        _timer.Tick += TimerTick;
        _timer.Start();
    }

    private void TimerTick(object? sender, EventArgs e)
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

        switch (e.EventName)
        {
            case EventType.MediaItemUpdated:
                mediaEvent = (MediaItemEventResponse)e;
                var item = QueuedItems.Items.FirstOrDefault(i => i.MediaItem?.Uri == mediaEvent.data!.Uri);
                item?.MediaItem?.Favorite = mediaEvent!.data!.Favorite;

                if (SelectedQueue?.CurrentItem?.MediaItem?.Uri == mediaEvent?.data?.Uri)
                {
                    SelectedQueue?.CurrentItem?.MediaItem?.Favorite = mediaEvent!.data!.Favorite;
                }

                break;
            case EventType.MediaItemPlayed:
                mediaEvent2 = (MediaItemEvent2Response)e;
                if (mediaEvent2.object_id == SelectedQueue?.CurrentItem?.MediaItem?.Uri)
                    if (mediaEvent2.data!.SecondsPlayed != null)
                        SelectedQueue?.CurrentItem?.MediaItem?.ElapsedTime = mediaEvent2.data.SecondsPlayed.Value;
                break;

            case EventType.QueueTimeUpdated:
                timeEvent = (PlayerQueueTimeUpdatedEventResponse)e;
                if (SelectedQueue != null && e.object_id == SelectedQueue.QueueId)
                {
                    SelectedQueue.CurrentItem!.MediaItem!.ElapsedTime = timeEvent.data;
                }
                break;

            case EventType.PlayerAdded:
                playerEvent = (PlayerEventResponse)e;
                if (!Players.Contains((playerEvent.data!)) && playerEvent.data != null)
                    Players.Add(playerEvent.data);
                break;

            case EventType.PlayerUpdated:
                playerEvent = (PlayerEventResponse)e;
                var player = Players.FirstOrDefault(p => p.PlayerId == playerEvent.data!.PlayerId);

                if (player != null)
                {
                    player.PlaybackState = playerEvent.data!.PlaybackState;
                    player.CurrentMedia = playerEvent.data.CurrentMedia;

                    var serverVol = playerEvent.data.VolumeLevel;

                    // If we have a recent local change for this player, and server value is close, treat as echo (ack).
                    if (_pendingLocalVolumeChanges.TryGetValue(player.PlayerId!, out var pending))
                    {
                        var age = DateTime.UtcNow - pending.Timestamp;
                        var diff = Math.Abs(pending.Volume - (int)serverVol!);

                        if (age < EchoIgnoreWindow && diff <= VolumeTolerance)
                        {
                            // Echo/ack: clear pending and keep optimistic local value (no UI bounce).
                            _pendingLocalVolumeChanges.TryRemove(player.PlayerId!, out _);
                        }
                        else
                        {
                            // Not an echo: apply authoritative server value and clear any pending (stale).
                            player.VolumeLevel = serverVol;
                            _pendingLocalVolumeChanges.TryRemove(player.PlayerId!, out _);
                        }
                    }
                    else
                    {
                        // No local pending change: apply server value directly.
                        player.VolumeLevel = serverVol;
                    }
                }
                break;

            case EventType.PlayerRemoved:
                playerEvent = (PlayerEventResponse)e;
                Players.RemoveAll(p => p.PlayerId == playerEvent?.data?.PlayerId);
                break;

            case EventType.QueueAdded:

                queueEvent = (PlayerQueueEventResponse)e;
                var existing = Queues.FirstOrDefault(q => q!.QueueId == queueEvent?.data?.QueueId);
                if (existing == null)
                    Queues.Add(queueEvent.data!);
                else
                    Queues.ReplaceOrAdd(existing, queueEvent.data);
                break;

            case EventType.QueueUpdated:
                queueEvent = (PlayerQueueEventResponse)e;
                if (SelectedQueue != null && queueEvent?.data?.QueueId == SelectedQueue.QueueId)
                {
                    SelectedQueue.ShuffleEnabled = queueEvent!.data!.ShuffleEnabled;
                    SelectedQueue.RepeatMode = queueEvent.data.RepeatMode;
                    SelectedQueue.CurrentIndex = queueEvent.data.CurrentIndex;
                    SelectedQueue.CurrentItem = queueEvent.data.CurrentItem;
                    var currentItem = SelectedQueue.CurrentItem;
                    if (currentItem == null)
                        break;

                    //Get the new background colours
                    if (currentItem.Image != null && currentItem.Image.RemotelyAccessible)
                        _ = _colourService.Update(currentItem.MediaItem!.ItemId!, currentItem.Image!.Path!);
                    else
                    {
                        var url = ImagePathHelper.ProxyString(currentItem.Image!.Path!, currentItem.Image!.Provider!);
                        _ = _colourService.Update(currentItem.MediaItem!.ItemId!, url);
                    }
                }
                break;
            case EventType.QueueItemsUpdated:
                _ = FetchQueueContentsAsync();
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
            if (playersResponse.Result != null)
                foreach (var y in playersResponse.Result)
                {
                    if (y != null)
                        Players.Add(y);
                }

            var queuesResponse = await _massClient.WithWs().GetPlayerQueuesAllAsync();
            if (queuesResponse.Result != null)
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

    public async Task PlayerChangeBy(int delta, Player? p = null)
    {
        p ??= SelectedPlayer;
        if (p == null)
            return;

        await _volumeSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!p.VolumeLevel.HasValue)
                return;

            var current = (int)Math.Round((decimal)p.VolumeLevel);
            var newVol = Math.Clamp(current + delta, 0, 100);

            p.VolumeLevel = newVol;

            _pendingLocalVolumeChanges[p.PlayerId!] = (newVol, DateTime.UtcNow);

            try
            {
                await _massClient.WithWs().SetPlayerGroupVolumeAsync(p.PlayerId!, newVol).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting volume for player {PlayerId}", p.PlayerId);
            }
        }
        finally
        {
            _volumeSemaphore.Release();
        }
    }

    public async Task PlayerVolume(int volume, Player? p = null)
    {
        p ??= SelectedPlayer;
        if (p == null)
            return;

        await _volumeSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var newVol = Math.Clamp(volume, 0, 100);

            // Optimistically update local model
            p.VolumeLevel = newVol;

            // Record pending local change per-player
            _pendingLocalVolumeChanges[p.PlayerId!] = (newVol, DateTime.UtcNow);

            try
            {
                await _massClient.WithWs().SetPlayerGroupVolumeAsync(p.PlayerId!, newVol).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting volume for player {PlayerId}", p.PlayerId);
            }
        }
        finally
        {
            _volumeSemaphore.Release();
        }
    }

    public async Task PlayerVolumeDown(Player? p = null)
    {
        p ??= SelectedPlayer;
        if (p == null)
            return;

        // Use delta path for consistent behavior (assumes volume step of 1)
        await PlayerChangeBy(-1, p).ConfigureAwait(false);
    }

    public async Task PlayerVolumeUp(Player? p = null)
    {
        p ??= SelectedPlayer;
        if (p == null)
            return;

        await PlayerChangeBy(1, p).ConfigureAwait(false);
    }

    public async Task PlayerPlayPause(Player? p = null)
    {
        p ??= SelectedPlayer;
        if (p == null)
            return;
        try
        {
            await _massClient.WithWs().PlayerPlayPauseAsync(p.PlayerId!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling playback for player {PlayerId}", p?.PlayerId);
        }
    }

    public async Task PlayItem(MediaItemBase t, Player? p = null, PlayMode mode = PlayMode.Replace, bool RadioMode = false)
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
                var pq = await _massClient.WithWs().GetPlayerActiveQueueAsync(p.PlayerId!);
                if (pq != null && pq.Result != null && !string.IsNullOrEmpty(pq.Result.QueueId))
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
            _ = _massClient.WithWs().PlayerNextAsync(p.PlayerId);
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
            _ = _massClient.WithWs().PlayerSeekAsync(p.PlayerId, position);
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
            _ = _massClient.WithWs().PlayerPreviousAsync(p.PlayerId);
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

    public async Task PlayAlbumRadio(Album album)
    {
        _ = PlayItem(album, RadioMode: true);
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
        _timer = null!;
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

    private TrackViewModel? GetOrCreateTrackViewModel(QueuedItem? queuedItem)
    {
        if (queuedItem == null)
            return null;

        var cacheKey = queuedItem.QueueItemId;

        if (cacheKey == null)
            return null;

        if (_trackViewModelCache.TryGetValue(cacheKey, out var cachedViewModel))
        {
            return cachedViewModel;
        }

        var trackViewModel = new TrackViewModel(_massClient, this, queuedItem.MediaItem);
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
        p ??= SelectedPlayer;

        await _massClient.WithWs().SetPlayerQueueRepeatAsync(p?.ActiveSource!, repeatmode);
    }

    public async Task PlayerShuffle(Player? p = null, bool shuffle = true)
    {
        p ??= SelectedPlayer;
        if (p?.PlayerId == null)
            return;
        try
        {
            _ = _massClient.WithWs().SetPlayerQueueShuffleAsync(p.ActiveSource!, shuffle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Caught exception");
        }
    }
    public async Task PlayerDontStopTheMusic(Player? p = null, bool dontstop = true)
    {
        p ??= SelectedPlayer;
        if (p?.PlayerId == null)
            return;
        try
        {
            _ = _massClient.WithWs().SetPlayerQueueDontStopTheMusicAsync(p.ActiveSource!, dontstop);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Caught exception");
        }
    }

    public async Task PlayerClearQueue(Player? p = null)
    {
        p ??= SelectedPlayer;
        if (p?.PlayerId == null)
            return;
        try
        {
            _ = _massClient.WithWs().ClearPlayerQueueAsync(p.ActiveSource!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Caught exception");
        }
    }

    /*
    public async Task FetchLyrics(Item t)
    {
        var lyricsResponse = await _massClient.WithWs().GetLyricsAsync(t);
        var lyrics = lyricsResponse.Result;
    }*/
    }