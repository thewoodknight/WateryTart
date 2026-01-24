using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Responses;

public class TracksResponse : ResponseBase
{
    public List<Track> result { get; set; }
}