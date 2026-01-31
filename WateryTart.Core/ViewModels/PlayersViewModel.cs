using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using WateryTart.Core.Services;

using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public partial class PlayersViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    private readonly IPlayersService _playersService;
    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
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