namespace WateryTart.Service.MassClient.Models;

public class QueuedItem : Item
{
    public string? queue_id { get; set; }
    public string? queue_item_id { get; set; }
    public int sort_index { get; set; }
    public Streamdetails? streamdetails { get; set; }
    public MediaItem? media_item { get; set; }
    public int index { get; set; }
}



