using WateryTart.Service.MassClient.Models.Auth;

namespace WateryTart.Core.Settings;

public interface ISettings
{
    public string Path { get; set; }
    public IMassCredentials Credentials { get; set; }

    public string LastSelectedPlayerId { get; set; }

    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }
    public double WindowPosX { get; set; }
    public double WindowPosY { get; set; }
}
