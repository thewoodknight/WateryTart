using WateryTart.MassClient.Models.Auth;

namespace WateryTart.MassClient.Responses;

public class AuthResponse : ResponseBase<AuthUser>
{
   // public AuthUser result { get; set; }
}

public class AuthUser
{
    public bool success { get; set; }
    public string access_token { get; set; }
    public User user { get; set; }
}