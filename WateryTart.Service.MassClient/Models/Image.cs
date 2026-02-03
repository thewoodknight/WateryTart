namespace WateryTart.Service.MassClient.Models;

public class Image
{
    public string? type { get; set; }
    public string? path { get; set; }
    public string? provider { get; set; }
    public bool remotely_accessible { get; set; }
}