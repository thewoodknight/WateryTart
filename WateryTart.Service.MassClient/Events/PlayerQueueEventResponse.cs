using WateryTart.Service.MassClient.Models;

namespace WateryTart.Service.MassClient.Events;

public class PlayerQueueEventResponse : BaseEventResponse
{
    public PlayerQueue? data { get; set; }
}

public class PlayerQueueTimeUpdatedEventResponse : BaseEventResponse
{
    public int data { get; set; }
}