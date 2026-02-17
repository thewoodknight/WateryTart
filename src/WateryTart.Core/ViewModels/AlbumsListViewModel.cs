using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.ViewModels;

public partial class AlbumsListViewModel : ReactiveObject, IViewModelBase
{
    private const int PageSize = 50;
    private readonly MusicAssistantClient _massClient;
    private readonly PlayersService _playersService;
    public ObservableCollection<AlbumViewModel> Albums { get; set; }
    [Reactive] public partial int CurrentOffset { get; set; } = 0;
    public RelayCommand GoNext { get; }
    [Reactive] public partial bool HasMoreItems { get; set; } = true;
    public IScreen HostScreen { get; }
    [Reactive] public partial bool IsLoading { get; set; }
    public AsyncRelayCommand LoadMoreCommand { get; }
    public AlbumViewModel? SelectedAlbum { get; set; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
    [Reactive] public partial string Title { get; set; } = string.Empty;
    public string? UrlPathSegment { get; } = "AlbumsList";

    public AlbumsListViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Albums = new ObservableCollection<AlbumViewModel>();

        GoNext = new RelayCommand(() =>
        {
            screen.Router.Navigate.Execute(SelectedAlbum);
            SelectedAlbum.Load(SelectedAlbum.Album);
            //SelectedAlbum = null;
        });

        LoadMoreCommand = new AsyncRelayCommand(
            LoadMoreAsync,
            () => !IsLoading && HasMoreItems
        );

#pragma warning disable CS4014
        _ = LoadInitialAsync();
#pragma warning restore CS4014
    }

    private async Task LoadAlbumsAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;

            var response = await _massClient.WithWs().GetMusicAlbumsLibraryItemsAsync(limit: PageSize, offset: CurrentOffset);

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
}