using DynamicData;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WateryTart.Core.ViewModels;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;

namespace WateryTart.Core.Services;

public interface IPlayersService
{
    double Progress { get; }
    PlayerQueue SelectedQueue { get; set; }
    ObservableCollection<Player> Players { get; }
    ReadOnlyObservableCollection<TrackViewModel> CurrentQueue { get; }
    ReadOnlyObservableCollection<TrackViewModel> PlayedQueue { get; }
    SourceCache<QueuedItem, string> QueuedItems { get; set; }
    Player? SelectedPlayer { get; set; }
    Task GetPlayers();
    Task PlayItem(MediaItemBase t, Player? p = null, PlayerQueue? q = null, PlayMode mode = PlayMode.Play, bool RadioMode = false);
    
    Task PlayerVolume(int volume, Player? p = null);
    Task PlayerVolumeDown(Player? p = null);
    Task PlayerVolumeUp(Player? p = null);
    Task PlayerPlayPause(Player? p = null);
    Task PlayerNext(Player? p = null);
    Task PlayerPrevious(Player? p = null);
    Task PlayArtistRadio(Artist artist);

    Task PlayerSeek(Player? p, int position);
    Task PlayerRemoveFromFavorites(MediaItemBase item);
    Task PlayerAddToFavorites(MediaItemBase item);

    Task PlayerSetRepeatMode(RepeatMode repeatmode);
}