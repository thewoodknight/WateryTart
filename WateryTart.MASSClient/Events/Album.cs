using Newtonsoft.Json;
using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Events;

public class EventResponse
{
    [JsonProperty("event")]
    public string EventName { get; set; }
    public string object_id { get; set; }

    public PlayerQueue data { get; set; }
}
