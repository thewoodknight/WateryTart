using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Responses;

public class RecommendationResponse : MediaItemBase
{
    public string path { get; set; }
    public object image { get; set; }
    public string icon { get; set; }
    public List<Item> items { get; set; }
    public string subtitle { get; set; }
}