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

public partial class PlaylistsViewModel : ViewModelBase<PlaylistsViewModel>
{
    [Reactive] public partial ObservableCollection<PlaylistViewModel> Playlists { get; set; } = new();
    [Reactive] public partial bool HasMoreItems { get; set; } = true;
    [Reactive] public partial int CurrentOffset { get; set; } = 0;

    [Reactive] public override partial bool IsLoading { get; set; }

    private const int PageSize = 50;

    public RelayCommand<PlaylistViewModel> ClickedCommand { get; }
    public AsyncRelayCommand LoadMoreCommand { get; }
    public PlaylistsViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService, ILoggerFactory loggerFactory)
            : base(loggerFactory, massClient, playersService, screen)
    {
        Title = "Playlists";

        ClickedCommand = new RelayCommand<PlaylistViewModel>(item =>
        {
            if (item?.Playlist.ItemId == null || item.Playlist?.Provider == null)
                return;

            item.LoadFromId(item.Playlist.ItemId, item.Playlist.Provider);
            HostScreen.Router.Navigate.Execute(item);
        }, item => item != null);

        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync, CanLoadMore);

        this.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(IsLoading) or nameof(HasMoreItems))
            {
                LoadMoreCommand.NotifyCanExecuteChanged();
            }
        };

        _ = LoadInitialAsync();
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
            
            var response = await _client.WithWs().GetPlaylistsAsync(limit: PageSize, offset: CurrentOffset);
            
            if (response?.Result != null)
            {
                foreach (var playlist in response.Result)
                {
                    Playlists.Add(new PlaylistViewModel(_client, HostScreen, _playersService!, playlist));
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