namespace WateryTart.Service.MassClient.Models;

public class QueuedItem
{
    public string? queue_id { get; set; }
    public string? queue_item_id { get; set; }
    public string? name { get; set; }
    public int duration { get; set; }


    public int sort_index { get; set; }
    public Streamdetails? streamdetails { get; set; }
    public MediaItem? media_item { get; set; }
    public Image? image { get; set; }
    public int index { get; set; }
    public bool available { get; set; }
}



