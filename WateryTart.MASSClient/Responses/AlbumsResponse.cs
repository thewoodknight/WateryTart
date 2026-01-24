using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Responses;

public class AlbumsResponse : ResponseBase
{
    public List<Album> result { get; set; }
}