namespace WateryTart.Service.MassClient.Models;

public class Metadata
{
    public string? description { get; set; }
    public string? review { get; set; }
    public bool? @explicit { get; set; }
    public List<Image>? images { get; set; }
    public object? grouping { get; set; }
    public List<string>? genres { get; set; }
    public string? mood { get; set; }
    public string? style { get; set; }
    public string? copyright { get; set; }
    public object? lyrics { get; set; }
    public object? lrc_lyrics { get; set; }
    public string? label { get; set; }
    public List<Link>? links { get; set; }
    public object? performers { get; set; }
    public object? preview { get; set; }
    public int? popularity { get; set; }
    public DateTime? release_date { get; set; }
    public object? languages { get; set; }
    public object? chapters { get; set; }
    public int? last_refresh { get; set; }
}