using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Events;

public class PlayerQueueEventResponse : BaseEventResponse
{
    public PlayerQueue data { get; set; }
}
