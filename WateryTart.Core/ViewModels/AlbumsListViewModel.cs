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

public partial class AlbumsListViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;
    public string? UrlPathSegment { get; } = "AlbumsList";
    public IScreen HostScreen { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoNext { get; }

    public ObservableCollection<AlbumViewModel> Albums { get; set; }
    public AlbumViewModel SelectedAlbum { get; set; }
    public ReactiveCommand<Unit, IRoutableViewModel> SelectedItemChangedCommand { get; }
    
    [Reactive] public partial string Title { get; set; }
    [Reactive] public partial bool IsLoading { get; set; }
    [Reactive] public partial bool HasMoreItems { get; set; } = true;
    [Reactive] public partial int CurrentOffset { get; set; } = 0;
    
    private const int PageSize = 50;
    
    public ReactiveCommand<Unit, Unit> LoadMoreCommand { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;

    public AlbumsListViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Albums = new ObservableCollection<AlbumViewModel>();

        GoNext = ReactiveCommand.CreateFromObservable(() =>
            {
                var e = screen.Router.Navigate.Execute(SelectedAlbum);
                SelectedAlbum.Load(SelectedAlbum.Album);
                SelectedAlbum = null;
                return e;
            }
        );

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
        Albums.Clear();
        await LoadAlbumsAsync();
    }

    private async Task LoadMoreAsync()
    {
        await LoadAlbumsAsync();
    }

    private async Task LoadAlbumsAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            
            var response = await _massClient.MusicAlbumsLibraryItemsAsync(limit: PageSize, offset: CurrentOffset);
            
            if (response?.Result != null)
            {
                foreach (var album in response.Result)
                {
                    Albums.Add(new AlbumViewModel(_massClient, HostScreen, _playersService, album));
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
            Debug.WriteLine($"Error loading albums: {ex.Message}");
            HasMoreItems = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}