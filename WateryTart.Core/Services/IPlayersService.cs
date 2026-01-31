using DynamicData;
using System.Collections.ObjectModel;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.Services;

public interface IPlayersService
{
    PlayerQueue SelectedQueue { get; set; }
    ObservableCollection<Player> Players { get; }
    ReadOnlyObservableCollection<QueuedItem> CurrentQueue { get; }
    ReadOnlyObservableCollection<QueuedItem> PlayedQueue { get; }
    SourceList<QueuedItem> QueuedItems { get; set; }
    Player SelectedPlayer { get; set; }
    void GetPlayers();
    void PlayItem(MediaItemBase t, Player? p = null, PlayerQueue? q = null, PlayMode mode = PlayMode.Play);
    void PlayerVolumeDown(Player? p = null);
    void PlayerVolumeUp(Player? p = null);
    void PlayerPlayPause(Player? p = null);
    void PlayerNext(Player? p = null);
    void PlayerPrevious(Player? p = null);
}