using System.Text.Json.Serialization;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Service.MassClient.Events;

public class PlayerEventResponse : BaseEventResponse
{
    [JsonPropertyName("data")]
    public new Player? data { get; set; }
}