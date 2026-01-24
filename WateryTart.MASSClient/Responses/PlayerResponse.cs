using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Responses;
public class PlayerResponse : ResponseBase
{
    public List<Player> result { get; set; }
}