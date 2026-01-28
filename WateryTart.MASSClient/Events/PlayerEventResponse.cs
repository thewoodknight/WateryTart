using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Events;

public class PlayerEventResponse : BaseEventResponse
{
    public Player data { get; set; }
}
