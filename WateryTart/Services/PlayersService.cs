using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using WateryTart.MassClient;
using WateryTart.MassClient.Models;
using WateryTart.MassClient.Responses;

namespace WateryTart.Services;

public class PlayersService : IPlayersService
{
    private readonly IMassWSClient _massClient;
    public ObservableCollection<Player> Players { get; set; }

    public ObservableCollection<PlayerQueue> Queues { get; set; }

    public PlayersService(IMassWSClient massClient)
    {
        _massClient = massClient;

        Players = new ObservableCollection<Player>();
        Queues = new ObservableCollection<PlayerQueue>();
    }

    public void GetPlayers()
    {
        _massClient
            .PlayersAll((a) =>
            {
                PlayerResponse x = (PlayerResponse)a;
                foreach (var y in x.result)
                {
                    Debug.WriteLine(y.display_name);
                }
            });

        _massClient.PlayerQueuesAll(a =>
        {
            foreach (var y in a.result)
            {
                Queues.Add(y);
                Debug.WriteLine(y.display_name);
            }
        });
    }

    public void Play(Item t)
    {
        var q = Queues.FirstOrDefault(pq => pq.display_name == "Web (Firefox on Windows)");

        _massClient.Play(q.queue_id, t, (a) =>
        {
            Debug.WriteLine(a);
        });
    }
}