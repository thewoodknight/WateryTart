using WateryTart.MassClient.Models.Auth;

namespace WateryTart.Settings;


public interface ISettings
{
    public IMassCredentials Credentials { get; set; }

}