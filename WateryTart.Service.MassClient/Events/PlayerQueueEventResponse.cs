using WateryTart.Service.MassClient.Models;

namespace WateryTart.Service.MassClient.Events;

public class PlayerQueueEventResponse : BaseEventResponse
{
    public PlayerQueue data { get; set; }
}
