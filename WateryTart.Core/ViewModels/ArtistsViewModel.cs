using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using WateryTart.Core.Services;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.WebSocketExtensions;

namespace WateryTart.Core.ViewModels;

public partial class ArtistsViewModel : ReactiveObject, IViewModelBase
{
    public string? UrlPathSegment { get; } = "ArtistsList";
    public IScreen HostScreen { get; }
    private readonly IWsClient _massClient;
    private readonly IPlayersService _playersService;

    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial ObservableCollection<ArtistViewModel> Artists { get; set; } = [];
    [Reactive] public partial bool IsLoading { get; set; }
    [Reactive] public partial bool HasMoreItems { get; set; } = true;
    [Reactive] public partial int CurrentOffset { get; set; } = 0;
    
    private const int PageSize = 50;

    public RelayCommand<ArtistViewModel> ClickedCommand { get; }
    public AsyncRelayCommand LoadMoreCommand { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;

    public ArtistsViewModel(IWsClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Title = "Artists";

        ClickedCommand = new RelayCommand<ArtistViewModel>(item =>
        {
            if (item != null)
                screen.Router.Navigate.Execute(item);
        });

        LoadMoreCommand = new AsyncRelayCommand(
            LoadMoreAsync,
            () => !IsLoading && HasMoreItems
        );

#pragma warning disable CS4014
        _ = LoadInitialAsync();
#pragma warning restore CS4014
    }

    private async Task LoadInitialAsync()
    {
        CurrentOffset = 0;
        Artists.Clear();
        await LoadArtistsAsync();
    }

    private async Task LoadMoreAsync()
    {
        await LoadArtistsAsync();
    }

    private async Task LoadArtistsAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            
            var response = await _massClient.ArtistsGetAsync(limit: PageSize, offset: CurrentOffset);

            if (response?.Result != null)
            {
                foreach (var artist in response.Result)
                {
                    Artists.Add(new ArtistViewModel(_massClient, HostScreen, _playersService, artist));
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
            Debug.WriteLine($"Error loading artists: {ex.Message}");
            HasMoreItems = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}