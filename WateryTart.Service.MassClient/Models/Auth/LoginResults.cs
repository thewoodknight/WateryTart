namespace WateryTart.Service.MassClient.Models.Auth;

public class LoginResults
{
    public MassCredentials? Credentials { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
}