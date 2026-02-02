using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;
using WateryTart.Core.Services;

namespace WateryTart.Core.ViewModels.Players;

public partial class BigPlayerViewModel : ReactiveObject, IViewModelBase
{
    private readonly IPlayersService _playersService;
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => false;
    public string Title { get; set; } = "";

    [Reactive] public partial bool IsSmallDisplay { get; set; }

    public IPlayersService PlayersService => _playersService;

    public ReactiveCommand<Unit, Unit> PlayerPlayPauseCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayerPreviousCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayNextCommand { get; }

    public BigPlayerViewModel(IPlayersService playersService, IScreen screen)
    {
        _playersService = playersService;
        HostScreen = screen;

        PlayerPlayPauseCommand = ReactiveCommand.CreateFromTask(() => _playersService.PlayerPlayPause(_playersService.SelectedPlayer));
        PlayerPreviousCommand = ReactiveCommand.CreateFromTask(() => _playersService.PlayerPrevious(_playersService.SelectedPlayer));
        PlayNextCommand = ReactiveCommand.CreateFromTask(() => _playersService.PlayerNext(_playersService.SelectedPlayer));
    }
}