namespace WateryTart.MassClient.Models;

public class CurrentMedia
{
    public string uri { get; set; }
    public string media_type { get; set; }
    public string title { get; set; }
    public string artist { get; set; }
    public string album { get; set; }
    public string image_url { get; set; }
    public double duration { get; set; }
    public string source_id { get; set; }
    public string queue_item_id { get; set; }
    public object custom_data { get; set; }
    public double? elapsed_time { get; set; }
    public double? elapsed_time_last_updated { get; set; }
}