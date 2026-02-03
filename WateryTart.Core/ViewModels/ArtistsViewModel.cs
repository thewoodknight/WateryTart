using DynamicData;
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

public partial class ArtistsViewModel : ReactiveObject, IViewModelBase
{
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;

    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial ObservableCollection<ArtistViewModel> Artists { get; set; } = new();
    [Reactive] public partial bool IsLoading { get; set; }
    [Reactive] public partial bool HasMoreItems { get; set; } = true;
    [Reactive] public partial int CurrentOffset { get; set; } = 0;
    
    private const int PageSize = 50;

    public ReactiveCommand<ArtistViewModel, Unit> ClickedCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;

    public ArtistsViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Title = "Artists";

        ClickedCommand = ReactiveCommand.Create<ArtistViewModel>(item =>
        {
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