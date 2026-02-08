using System.Linq;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive.Linq;
using System.Windows.Input;
using Material.Icons;
using Material.Icons.Avalonia;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;

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
            var GoToAlbum = new RelayCommand(() => {});
            var GoToArtist = new RelayCommand(() => { });
            var item = PlayersService.SelectedQueue.CurrentItem.MediaItem;
            
            var menu = new MenuViewModel(
            [
                new MenuItemViewModel("Go to Album", MaterialIconKind.Album, GoToAlbum),
                new TwoLineMenuItemViewModel("Go to Artist", item.Artists.FirstOrDefault().Name, MaterialIconKind.Artist, GoToArtist),

            ]);
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