using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Responses;

public class PlayerQueueResponse : ResponseBase
{
    public List<PlayerQueue> result { get; set; }
    
}