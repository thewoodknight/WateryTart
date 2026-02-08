using System.Linq;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive.Linq;
using System.Windows.Input;
using Material.Icons;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels.Players;

public partial class BigPlayerViewModel : ReactiveObject, IViewModelBase
{
    private readonly IPlayersService _playersService;
    public string? UrlPathSegment { get; } = "BigPlayer";
    public required IScreen HostScreen { get; set; }
    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => false;
    public bool ShowBackButton => false;
    public string Title { get; set; } = "";
    [Reactive] public partial IColourService ColourService { get; set; }
    [Reactive] public partial bool IsSmallDisplay { get; set; }
    public double CachedImageWidth
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double CachedImageHeight
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public IPlayersService PlayersService => _playersService;

    public ICommand PlayerNextCommand { get; set; }
    public ICommand PlayerPlayPauseCommand { get; set; }
    public ICommand PlayPreviousCommand { get; set; }
    public ICommand PlayingAltMenuCommand { get; set; }

    public BigPlayerViewModel(IPlayersService playersService, IScreen screen, IColourService colourService)
    {
        PlayPreviousCommand = new RelayCommand(() => PlayersService.PlayerPrevious());
        PlayerNextCommand = new RelayCommand(() => PlayersService.PlayerNext());
        PlayerPlayPauseCommand = new RelayCommand(() => PlayersService.PlayerPlayPause());
        PlayingAltMenuCommand = new RelayCommand(() =>
        {
            var item = PlayersService?.SelectedQueue?.CurrentItem?.MediaItem;

            var GoToAlbum = new RelayCommand(() =>
            {
                if (item == null || HostScreen == null)
                    return;

                var albumVm = App.Container.GetRequiredService<AlbumViewModel>();
                albumVm.Album = item.Album;
                albumVm.LoadFromId(item.Album.ItemId, item.Provider);
                HostScreen.Router.Navigate.Execute(albumVm);

            });

            var GoToArtist = new RelayCommand(() =>
            {
                if (item == null || HostScreen == null)
                    return;
                var artistVm = App.Container.GetRequiredService<ArtistViewModel>();
                artistVm.LoadFromId(item.Artists[0].ItemId, item.Provider);
                HostScreen.Router.Navigate.Execute(artistVm);
            });

            var GoToSimilarTracks = new RelayCommand(() =>
            {

                if (item == null || HostScreen == null)
                    return;
                var SimilarTracksViewModel = App.Container.GetRequiredService<SimilarTracksViewModel>();
                SimilarTracksViewModel.LoadFromId(item.ItemId, item.GetProviderInstance());
                HostScreen.Router.Navigate.Execute(SimilarTracksViewModel);
            });


            var menu = new MenuViewModel(
            [
                new TwoLineMenuItemViewModel("Go to Album", item.Album.Name, MaterialIconKind.Album, GoToAlbum),
                new TwoLineMenuItemViewModel("Go to Artist", item.Artists.FirstOrDefault().Name, MaterialIconKind.Artist, GoToArtist),
                new MenuItemViewModel("Similar tracks", MaterialIconKind.MusicClefTreble, GoToSimilarTracks),

            ], PlayersService.SelectedQueue.CurrentItem);
            MessageBus.Current.SendMessage(menu);
        });
        _playersService = playersService;
        ColourService = colourService;
        HostScreen = screen;

        // Create a CanExecute observable that checks if a player is selected
        var canExecute = this.WhenAnyValue(x => x._playersService.SelectedPlayer)
            .Select(player => player != null)
            .ObserveOn(RxApp.MainThreadScheduler)
            .DistinctUntilChanged();
    }

    public void UpdateCachedDimensions(double width, double height)
    {
        if (width > 0 && height > 0)
        {
            CachedImageWidth = width;
            CachedImageHeight = height;
        }
    }
}