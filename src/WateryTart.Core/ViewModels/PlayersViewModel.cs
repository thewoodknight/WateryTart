using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using WateryTart.Core.Services;

using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.ViewModels;

public partial class PlayersViewModel : ReactiveObject, IViewModelBase
{
    private readonly PlayersService _playersService;
    public string? UrlPathSegment { get; } = "players";
    public IScreen HostScreen { get; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;
    [Reactive] public partial ObservableCollection<Player> Players { get; set; }
    [Reactive] public partial bool IsLoading { get; set; } = false;
    public Player? SelectedPlayer
    {
        get => field;
        set
        {
            _playersService.SelectedPlayer = value;
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    public PlayersViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService)
    {
        _playersService = playersService;
        HostScreen = screen;
        Players = playersService.Players;
        SelectedPlayer = playersService.SelectedPlayer;
    }

    public string Title => "Players";
}