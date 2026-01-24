using System.Collections.ObjectModel;
using WateryTart.MassClient.Models;

namespace WateryTart.Services;

public interface IPlayersService
{
    public ObservableCollection<Player> Players { get; }
    public void GetPlayers();
    public void Play(Track t);
}