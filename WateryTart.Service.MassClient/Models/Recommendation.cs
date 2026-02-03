namespace WateryTart.Service.MassClient.Models;

public class Recommendation : MediaItemBase
{
    public string? path { get; set; }
    public new object? image { get; set; }
    public string? icon { get; set; }
    public List<Item>? items { get; set; }
    public string? subtitle { get; set; }
}