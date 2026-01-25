namespace WateryTart.MassClient.Models;

public class MediaItem : MediaItemBase
{
    public object position { get; set; }
    public int duration { get; set; }
    public List<Artist> artists { get; set; }
    public int last_played { get; set; }
    public Album album { get; set; }
    public int disc_number { get; set; }
    public int track_number { get; set; }
}