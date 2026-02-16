using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.MusicAssistant;
using CommunityToolkit.Mvvm.Input;
using WateryTart.MusicAssistant.WsExtensions;
using WateryTart.Core.ViewModels.Popups;

namespace WateryTart.Core.ViewModels;

public partial class TracksViewModel : ReactiveObject, IViewModelBase
{
    public string? UrlPathSegment { get; } = "Tracks";
    public IScreen HostScreen { get; }
    private readonly MusicAssistantClient _massClient;
    private readonly IPlayersService _playersService;
    private readonly ILogger _logger;

    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial ObservableCollection<TrackViewModel> Tracks { get; set; } = new();
    [Reactive] public partial bool IsLoading { get; set; }
    [Reactive] public partial bool HasMoreItems { get; set; } = true;
    [Reactive] public partial int CurrentOffset { get; set; } = 0;
    
    private const int PageSize = 50;

    public RelayCommand<TrackViewModel> ClickedCommand { get; }
    public AsyncRelayCommand LoadMoreCommand { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;

    public TracksViewModel(MusicAssistantClient massClient, IScreen screen, IPlayersService playersService, ILoggerFactory loggerFactory)
    {
        _massClient = massClient;
        _playersService = playersService;
        _logger = loggerFactory.CreateLogger<TracksViewModel>();
        HostScreen = screen;
        Title = "Tracks";

        ClickedCommand = new RelayCommand<TrackViewModel>(item =>
        {
            if (item == null)
                return;

            MessageBus.Current.SendMessage<IPopupViewModel>(MenuHelper.BuildStandardPopup(_playersService, item.Track));
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
        Tracks.Clear();
        await LoadTracksAsync();
    }

    private async Task LoadMoreAsync()
    {
        await LoadTracksAsync();
    }

    private async Task LoadTracksAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            
            var response = await _massClient.WithWs().GetTracksAsync(limit: PageSize, offset: CurrentOffset);
            
            if (response?.Result != null)
            {
                foreach (var track in response.Result)
                {
                    Tracks.Add(new TrackViewModel(_massClient, HostScreen, _playersService, track));
                }

                HasMoreItems = response.Result.Count == PageSize;
                
                if (HasMoreItems)
                {
                    CurrentOffset += PageSize;
                }
                
                _logger.LogInformation($"Loaded {response.Result.Count} tracks. Total: {Tracks.Count}. HasMore: {HasMoreItems}");
            }
            else
            {
                HasMoreItems = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading tracks");
            HasMoreItems = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}