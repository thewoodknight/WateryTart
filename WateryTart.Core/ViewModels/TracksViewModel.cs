using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public partial class TracksViewModel : ReactiveObject, IViewModelBase
{
    public string? UrlPathSegment { get; } = "Tracks";
    public IScreen HostScreen { get; }
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;

    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial ObservableCollection<TrackViewModel> Tracks { get; set; } = new();
    [Reactive] public partial bool IsLoading { get; set; }
    [Reactive] public partial bool HasMoreItems { get; set; } = true;
    [Reactive] public partial int CurrentOffset { get; set; } = 0;
    
    private const int PageSize = 50;

    public ReactiveCommand<TrackViewModel, Unit> ClickedCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;

    public TracksViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Title = "Tracks";

        ClickedCommand = ReactiveCommand.Create<TrackViewModel>(item =>
        {
            MessageBus.Current.SendMessage(MenuHelper.BuildStandardPopup(_playersService, item.Track));
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
            
            var response = await _massClient.TracksGetAsync(limit: PageSize, offset: CurrentOffset);
            
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
                
                Debug.WriteLine($"Loaded {response.Result.Count} tracks. Total: {Tracks.Count}. HasMore: {HasMoreItems}");
            }
            else
            {
                HasMoreItems = false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading tracks: {ex.Message}");
            HasMoreItems = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}