using WateryTart.Service.MassClient.Models;

namespace WateryTart.Service.MassClient.Events;

public class PlayerEventResponse : BaseEventResponse
{
    public Player data { get; set; }
}
