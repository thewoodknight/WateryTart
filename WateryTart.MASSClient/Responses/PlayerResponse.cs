using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Responses;

public class PlayerResponse : ResponseBase<List<Player>>
{
   // public List<Player> result { get; set; }
}

public class PlaylistResponse : ResponseBase<Playlist>
{
}

public class Playlist : MediaItemBase
{

}

