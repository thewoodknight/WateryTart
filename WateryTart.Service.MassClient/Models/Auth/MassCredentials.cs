using System.Text.Json.Serialization;

namespace WateryTart.Service.MassClient.Models.Auth;

public class MassCredentials : IMassCredentials
{
    [JsonPropertyName("Token")] 
    public string? Token { get; set; }

    [JsonPropertyName("BaseUrl")] 
    public string? BaseUrl { get; set; }
    
    [JsonPropertyName("Username")] 
    public string? Username { get; set; } = string.Empty;
}