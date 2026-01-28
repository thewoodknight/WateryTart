using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using WateryTart.MassClient;
using WateryTart.MassClient.Models;
using WateryTart.Services;

namespace WateryTart.ViewModels;

public partial class PlayersViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public bool ShowMiniPlayer { get => true; }
    [Reactive] public partial ObservableCollection<Player> Players { get; set; }

    public Player SelectedPlayer
    {
        get => field;
        set
        {
            _playersService.SelectedPlayer = value;
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    public PlayersViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService)
    {
        _massClient = massClient;
        _playersService = playersService;
        HostScreen = screen;
        Players = playersService.Players;
        SelectedPlayer = playersService.SelectedPlayer;
    }

    public string Title
    {
        get => "Players";
        set;
    }
}