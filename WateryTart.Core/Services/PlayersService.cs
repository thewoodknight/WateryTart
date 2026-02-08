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
using WateryTart.Core.Extensions;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Events;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;
using WateryTart.MusicAssistant.WebSocketExtensions;

namespace WateryTart.Core.Services;

public partial class PlayersService : ReactiveObject, IPlayersService, IAsyncReaper
{
    private readonly IWsClient _massClient;
    private readonly ISettings _settings;
    private readonly IColourService _colourService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<PlayersService> _logger;
    private ReadOnlyObservableCollection<TrackViewModel> currentQueue;
    private ReadOnlyObservableCollection<TrackViewModel> playedQueue;
    private CompositeDisposable _subscriptions = new CompositeDisposable();
    private readonly Dictionary<string, TrackViewModel> _trackViewModelCache = new Dictionary<string, TrackViewModel>();

    [Reactive] public partial SourceList<QueuedItem> QueuedItems { get; set; } = new SourceList<QueuedItem>();
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
        var pq = await _massClient.PlayerActiveQueueAsync(id);
        SelectedPlayerQueueId = pq.Result.QueueId;
        SelectedQueue = pq.Result;
        await FetchQueueContentsAsync();
    }

    private async Task FetchQueueContentsAsync()
    {
        var items = await _massClient.PlayerQueueItemsAsync(SelectedPlayerQueueId);
        try
        {
            if (items.Result != null)
            {
                QueuedItems.Clear();
                QueuedItems.AddRange(items.Result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching queue contents");
        }
    }

    private DispatcherTimer _timer;

    public PlayersService(IWsClient massClient, ISettings settings, IColourService colourService, ILoggerFactory loggerFactory)
    {
        _massClient = massClient;
        _settings = settings;
        _colourService = colourService;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<PlayersService>();

        /* Subscribe to the relevant websocket events from MASS */
        _subscriptions.Add(_massClient.Events
            .ObserveOn(RxApp.MainThreadScheduler)
            .SelectMany(e => Observable.FromAsync(() => OnEvents(e)))
            .Subscribe());

        /* This takes care of filtering the two lists */
        _subscriptions.Add(QueuedItems
                .Connect()
                .Sort(SortExpressionComparer<QueuedItem>.Ascending(t => t.SortIndex))
                .Filter(i => SelectedQueue != null && (i.SortIndex > SelectedQueue.CurrentIndex || i.MediaItem?.ItemId == SelectedQueue.CurrentItem?.MediaItem?.ItemId))
                .Transform(i => GetOrCreateTrackViewModel(i))   //transform is expensive since the list is reset to 0, and despite caching still means the visual tree is recreated
                .Bind(out currentQueue)
                .Subscribe());


        _subscriptions.Add(QueuedItems
                .Connect()
                .Sort(SortExpressionComparer<QueuedItem>.Descending(t => t.SortIndex))
                .Filter(i => SelectedQueue != null && i.SortIndex < SelectedQueue.CurrentIndex)
                .Transform(i => GetOrCreateTrackViewModel(i))
                .Bind(out playedQueue)
                .Subscribe());

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
        switch (e.EventName)
        {
            case EventType.MediaItemPlayed:
                mediaEvent = (MediaItemEventResponse)e;
                if (mediaEvent.object_id == SelectedQueue?.CurrentItem?.MediaItem?.Uri)
                    if (mediaEvent.data.SecondsPlayed != null)
                        SelectedQueue?.CurrentItem?.MediaItem?.ElapsedTime = mediaEvent.data.SecondsPlayed.Value;
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

            case EventType.QueueUpdated:
                queueEvent = (PlayerQueueEventResponse)e;
                if (SelectedQueue != null && queueEvent.data.QueueId == SelectedQueue.QueueId)
                {
                    SelectedQueue.CurrentIndex = queueEvent.data.CurrentIndex;
                    SelectedQueue.CurrentItem = queueEvent.data.CurrentItem;

                    var currentItem = SelectedQueue.CurrentItem;
                    if (currentItem == null)
                        break;
                    if (currentItem.Image != null && currentItem.Image.RemotelyAccessible)
                        _ = _colourService.Update(currentItem.MediaItem.ItemId, currentItem.Image.Path);
                    else
                    {
                        var url = string.Format("http://{0}/imageproxy?path={1}&provider={2}&checksum=&size=256", App.BaseUrl, Uri.EscapeDataString(currentItem.Image.Path), currentItem.Image.Provider);
                        _ = _colourService.Update(currentItem.MediaItem.ItemId, url);
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
            _logger.LogError(ex, "Error loading players");
        }
    }

    public async Task PlayerVolumeDown(Player? p)
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
                _logger.LogError(ex, "Error adjusting volume down for player {PlayerId}", p.PlayerId);
            }
        }
    }

    public async Task PlayerVolumeUp(Player? p)
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
                _logger.LogError(ex, "Error adjusting volume up for player {PlayerId}", p.PlayerId);
            }
        }
    }

    public async Task PlayerPlayPause(Player? p)
    {
        p ??= SelectedPlayer;
        try
        {
            await _massClient.PlayerPlayPauseAsync(p.PlayerId);
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
            var menu = MenuHelper.AddPlayers(this, t);
            MessageBus.Current.SendMessage(menu);
        }
        else
        {
            try
            {
                _logger.LogInformation($"Playing {t.Name}"); 
                var pq = await _massClient.PlayerActiveQueueAsync(p.PlayerId);
                await _massClient.PlayAsync(pq.Result.QueueId, t, mode, RadioMode);
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
            _ = _massClient.PlayerNextAsync(p.PlayerId);
#pragma warning restore CS4014
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error playing next on player {PlayerId}", p.PlayerId);
        }
    }

    public async Task PlayerPrevious(Player? p)
    {
        p ??= SelectedPlayer;
        if (p?.PlayerId == null)
            return;
        try
        {
#pragma warning disable CS4014
            _ = _massClient.PlayerPreviousAsync(p.PlayerId);
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
            await _massClient.DisconnectAsync();
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
            var disconnectTask = _massClient.DisconnectAsync();
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
}