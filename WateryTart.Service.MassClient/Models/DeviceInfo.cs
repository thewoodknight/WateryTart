namespace WateryTart.Service.MassClient.Models;

public class DeviceInfo
{
    public string? model { get; set; }
    public string? manufacturer { get; set; }
    public string? software_version { get; set; }
    public object? model_id { get; set; }
    public object? manufacturer_id { get; set; }
    public string? ip_address { get; set; }
    public object? mac_address { get; set; }
}