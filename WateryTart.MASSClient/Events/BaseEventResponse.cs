using Newtonsoft.Json;
using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Events;

public class BaseEventResponse
{
    [JsonProperty("event")]
    public EventType EventName { get; set; }

    public string object_id { get; set; }
}