using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Material;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SharpHook.Testing;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.ViewModels.Popups;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.ViewModels;

public partial class AlbumViewModel : ReactiveObject, IViewModelBase
{
    private readonly MusicAssistantClient _massClient;
    private readonly PlayersService _playersService;
    private ProviderService _providerservice;
    [Reactive] public partial Album? Album { get; set; }
    public RelayCommand AlbumAltMenuCommand { get; }
    public RelayCommand AlbumFullViewCommand { get; }
    public IScreen HostScreen { get; }
    [Reactive] public partial string InputProviderIcon { get; set; }
    [Reactive] public partial bool IsLoading { get; set; }
    public AsyncRelayCommand PlayAlbumCommand { get; }
    public ICommand ArtistViewCommand { get; set; }
    public ICommand AlbumAltCommand { get; set; }
    [Reactive] public partial ProviderManifest Provider { get; set; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
    [Reactive] public partial string Title { get; set; } = string.Empty;
    [Reactive] public partial ObservableCollection<TrackViewModel> Tracks { get; set; }
    public AsyncRelayCommand<Item?> TrackTappedCommand { get; }
    public string? UrlPathSegment { get; } = "Album/ID";

    public AlbumViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService, Album? a = null)
    {
        _providerservice = App.Container.GetRequiredService<ProviderService>();
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Album = a;
        Tracks = new ObservableCollection<TrackViewModel>();

        AlbumAltCommand = new RelayCommand(() =>
        {
            if (Album == null)
                return;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var menu = new MenuViewModel(
            [
                //new MenuItemViewModel("Add to library", PackIconMaterialKind.BookPlusMultiple, addToLibraryCommand),
                //new MenuItemViewModel("Add to favourites", PackIconMaterialKind.HeartPlus, addToFavouritesCommand),
                new MenuItemViewModel("Start Radio",PackIconMaterialKind.RadioTower, new RelayCommand(() => _playersService.PlayAlbumRadio(Album)))
                //new MenuItemViewModel("Play", PackIconMaterialKind.Play, playCommand)
            ]);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            var playerMenuItems = MenuHelper.AddPlayers(playersService, Album);
            menu.AddMenuItem(playerMenuItems);


            MessageBus.Current.SendMessage<IPopupViewModel>(menu);
        });

        ArtistViewCommand = new RelayCommand(() =>
        {
            if (Album?.Artists != null && Album.Artists.Count > 0)
            {
                var artist = Album.Artists[0];
                if (artist.ItemId != null && artist.Provider != null)
                {
                    var vm = new ArtistViewModel(_massClient, HostScreen, _playersService);
                    vm.LoadFromId(artist.ItemId, artist.Provider);
                    screen.Router.Navigate.Execute(vm);
                }
            }
        });
        PlayAlbumCommand = new AsyncRelayCommand(async () =>
        {
            if (Album != null)
                if (_playersService.SelectedPlayer == null)
                {
                    MessageBus.Current.SendMessage<IPopupViewModel>(MenuHelper.BuildStandardPopup(_playersService, Album));
                    await Task.CompletedTask;
                }
                else
                {
                    await _playersService.PlayItem(Album, mode: PlayMode.Replace);
                }
        });

        TrackTappedCommand = new AsyncRelayCommand<Item?>((t) =>
        {
            if (t != null)
                return _playersService.PlayItem(t, mode: PlayMode.Replace);
            return null!;
        });
        AlbumFullViewCommand = new RelayCommand(() =>
        {
            if (Album.ItemId != null && Album.Provider != null)
                LoadFromId(Album.ItemId, Album.Provider);
            screen.Router.Navigate.Execute(this);
        });
        AlbumAltMenuCommand = new RelayCommand(() =>
        {
            MessageBus.Current.SendMessage<IPopupViewModel>(MenuHelper.BuildStandardPopup(playersService, Album));
        });
    }

    public void Load(Album album)
    {
        Album = album;
#pragma warning disable CS4014 // Fire-and-forget intentional - loads data asynchronously
        if (album.ItemId != null && album.Provider != null)
            _ = LoadAlbumDataAsync(album.ItemId, album.Provider);
#pragma warning restore CS4014
    }

    public void LoadFromId(string id, string provider)
    {
#pragma warning disable CS4014 // Fire-and-forget intentional - loads data asynchronously
        _ = LoadAlbumDataAsync(id, provider);
#pragma warning restore CS4014
    }

    private async Task LoadAlbumDataAsync(string id, string provider)
    {
        IsLoading = true;
        try
        {
            var albumResponse = await _massClient.WithWs().GetMusicAlbumAsync(id, provider);
            Album = albumResponse.Result;
            if (Album != null && Album.Name != null)
                Title = Album.Name;

            var domain = Album?.ProviderMappings?.FirstOrDefault()?.ProviderDomain;

            if (!string.IsNullOrEmpty(domain))
                Provider = _providerservice.GetProvider(domain);

            if (Provider != null)
                if (!string.IsNullOrEmpty(Provider.IconSvgDark))
                    InputProviderIcon = Provider.IconSvgDark;
                else InputProviderIcon = Provider.IconSvg;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading album: {ex.Message}");
        }

        try
        {
            var tracksResponse = await _massClient.WithWs().GetMusicAlbumTracksAsync(id, provider);
            if (tracksResponse.Result != null)
                foreach (var t in tracksResponse.Result)
                    Tracks.Add(new TrackViewModel(_massClient, HostScreen, _playersService, t));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading tracks: {ex.Message}");
        }
        IsLoading = false;
    }
}