namespace WateryTart.Service.MassClient.Models.Auth;

public class AuthUser
{
    public bool success { get; set; }
    public string? access_token { get; set; }

    public string? error { get; set; }
    public User? user { get; set; }
}