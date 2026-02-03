namespace WateryTart.Service.MassClient.Models;

public class SourceList
{
    public string? id { get; set; }
    public string? name { get; set; }
    public bool passive { get; set; }
    public bool can_play_pause { get; set; }
    public bool can_seek { get; set; }
    public bool can_next_previous { get; set; }
}