using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Responses;

public class TracksResponse : ResponseBase
{
    public List<Item> result { get; set; }
}