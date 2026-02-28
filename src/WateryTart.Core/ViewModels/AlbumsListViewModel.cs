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

public partial class AlbumsListViewModel : ViewModelBase<AlbumsListViewModel>
{
    private const int PageSize = 50;
    public ObservableCollection<AlbumViewModel> Albums { get; set; }
    [Reactive] public partial int CurrentOffset { get; set; } = 0;
    public RelayCommand GoNext { get; }
    [Reactive] public partial bool HasMoreItems { get; set; } = true;
    public AsyncRelayCommand LoadMoreCommand { get; }
    public AlbumViewModel? SelectedAlbum { get; set; }
    [Reactive] public override partial bool IsLoading { get; set; }
    public AlbumsListViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService)
        : base(null, massClient, playersService)
    {
        HostScreen = screen;
        Albums = [];
        Title = "Albums";

        GoNext = new RelayCommand(() =>
        {
            if (SelectedAlbum != null && SelectedAlbum.Album != null)
            {
                screen.Router.Navigate.Execute(SelectedAlbum);
                SelectedAlbum.Load(SelectedAlbum.Album);
            }
            //SelectedAlbum = null;
        });

        LoadMoreCommand = new AsyncRelayCommand(
            LoadMoreAsync,
            () => !IsLoading && HasMoreItems
        );

        _ = LoadInitialAsync();
    }

    private async Task LoadAlbumsAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;

            var response = await _client.WithWs().GetMusicAlbumsLibraryItemsAsync(limit: PageSize, offset: CurrentOffset);

            if (response?.Result != null)
            {
                foreach (var album in response.Result)
                {
                    Albums.Add(new AlbumViewModel(_client, HostScreen, _playersService, album));
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