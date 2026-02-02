using DynamicData;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
    Task GetPlayers();
    Task PlayItem(MediaItemBase t, Player? p = null, PlayerQueue? q = null, PlayMode mode = PlayMode.Play);
    Task PlayerVolumeDown(Player? p = null);
    Task PlayerVolumeUp(Player? p = null);
    Task PlayerPlayPause(Player? p = null);
    Task PlayerNext(Player? p = null);
    Task PlayerPrevious(Player? p = null);
}