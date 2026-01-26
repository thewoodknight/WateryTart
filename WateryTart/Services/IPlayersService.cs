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
    void PlayerVolumeDown(Player p);
    void PlayerVolumeUp(Player p);
    void PlayerPlayPause(Player p);
}