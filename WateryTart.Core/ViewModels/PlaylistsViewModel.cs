using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public partial class PlaylistsViewModel : ReactiveObject, IViewModelBase
{
    public string? UrlPathSegment { get; } = "Playlists";
    public IScreen HostScreen { get; }
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;

    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial ObservableCollection<PlaylistViewModel> Playlists { get; set; } = new();
    [Reactive] public partial bool IsLoading { get; set; }
    [Reactive] public partial bool HasMoreItems { get; set; } = true;
    [Reactive] public partial int CurrentOffset { get; set; } = 0;
    
    private const int PageSize = 50;

    public ReactiveCommand<PlaylistViewModel, Unit> ClickedCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;

    public PlaylistsViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Title = "Playlists";

        ClickedCommand = ReactiveCommand.Create<PlaylistViewModel>(item =>
        {
            item.LoadFromId(item.Playlist.ItemId, item.Playlist.Provider);
            screen.Router.Navigate.Execute(item);
        });

        LoadMoreCommand = ReactiveCommand.CreateFromTask(
            LoadMoreAsync,
            this.WhenAnyValue(x => x.IsLoading, x => x.HasMoreItems, (loading, hasMore) => !loading && hasMore)
        );

#pragma warning disable CS4014
        _ = LoadInitialAsync();
#pragma warning restore CS4014
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
            
            var response = await _massClient.PlaylistsGetAsync(limit: PageSize, offset: CurrentOffset);
            
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
            Debug.WriteLine($"Error loading playlists: {ex.Message}");
            HasMoreItems = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}