using System.Collections.ObjectModel;
using WateryTart.MassClient.Messages;
using WateryTart.MassClient.Models;
using WateryTart.ViewModels;

namespace WateryTart.Services;

public interface IPlayersService
{
    ObservableCollection<PlayerViewModel> Players2 { get; set; }
    ObservableCollection<Player> Players { get; }
    Player SelectedPlayer { get; set; }
    void GetPlayers();
    void PlayItem(MediaItemBase t, Player? p = null, PlayerQueue? q = null, PlayMode mode = PlayMode.Play);
    void PlayerVolumeDown(Player? p = null);
    void PlayerVolumeUp(Player? p = null);
    void PlayerPlayPause(Player? p = null);

    void PlayerNext(Player? p = null);
    void PlayerPrevious(Player? p = null);
}