using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WateryTart.Core.Services;
using WateryTart.MusicAssistant;
using CommunityToolkit.Mvvm.Input;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.ViewModels;

public partial class PlaylistsViewModel : ReactiveObject, IViewModelBase
{
    public string? UrlPathSegment { get; } = "Playlists";
    public IScreen HostScreen { get; }
    private readonly MusicAssistantClient _massClient;
    private readonly PlayersService _playersService;
    private readonly ILogger _logger;
    [Reactive] public partial bool IsLoading { get; set; } = false;
    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial ObservableCollection<PlaylistViewModel> Playlists { get; set; } = new();
    [Reactive] public partial bool HasMoreItems { get; set; } = true;
    [Reactive] public partial int CurrentOffset { get; set; } = 0;
    
    private const int PageSize = 50;

    public RelayCommand<PlaylistViewModel> ClickedCommand { get; }
    public AsyncRelayCommand LoadMoreCommand { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;

    public PlaylistsViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService, ILoggerFactory loggerFactory)
    {
        _massClient = massClient;
        _playersService = playersService;
        _logger = loggerFactory.CreateLogger<PlaylistsViewModel>();
        HostScreen = screen;
        Title = "Playlists";

        ClickedCommand = new RelayCommand<PlaylistViewModel>(item =>
        {
            if (item?.Playlist.ItemId == null || item.Playlist?.Provider == null)
                return;

            item.LoadFromId(item.Playlist.ItemId, item.Playlist.Provider);
            screen.Router.Navigate.Execute(item);
        }, item => item != null);

        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync, CanLoadMore);

        this.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(IsLoading) or nameof(HasMoreItems))
            {
                LoadMoreCommand.NotifyCanExecuteChanged();
            }
        };

#pragma warning disable CS4014
        _ = LoadInitialAsync();
#pragma warning restore CS4014
    }

    private bool CanLoadMore()
    {
        return !IsLoading && HasMoreItems;
    }

    private async Task LoadInitialAsync()
    {
        CurrentOffset = 0;
        Playlists.Clear();
        await LoadPlaylistsAsync();
    }

    private async Task LoadMoreAsync()
    {
        await LoadPlaylistsAsync();
    }

    private async Task LoadPlaylistsAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            
            var response = await _massClient.WithWs().GetPlaylistsAsync(limit: PageSize, offset: CurrentOffset);
            
            if (response?.Result != null)
            {
                foreach (var playlist in response.Result)
                {
                    Playlists.Add(new PlaylistViewModel(_massClient, HostScreen, _playersService, playlist));
                }

                HasMoreItems = response.Result.Count == PageSize;
                
                if (HasMoreItems)
                {
                    CurrentOffset += PageSize;
                }
            }
            else
            {
                HasMoreItems = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading playlists");
            HasMoreItems = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}