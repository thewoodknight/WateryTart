namespace WateryTart.Service.MassClient.Models.Auth;

public interface IMassCredentials
{
    public string? Token { get; set; }
    public string? BaseUrl { get; set; }
    public string? Username { get; set; }
}