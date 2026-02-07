using System.Text.Json.Serialization;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Service.MassClient.Events;

public class PlayerQueueEventResponse : BaseEventResponse
{
    [JsonPropertyName("data")]
    public new PlayerQueue? data { get; set; }
}