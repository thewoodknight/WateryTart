using Newtonsoft.Json;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Service.MassClient.Events;

public class BaseEventResponse
{
    [JsonProperty("event")]
    public EventType EventName { get; set; }

    public string object_id { get; set; }
}