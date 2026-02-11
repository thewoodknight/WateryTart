using WateryTart.MusicAssistant.Models.Auth;

namespace WateryTart.Core.Settings;

public interface ISettings
{
    public string Path { get; set; }
    public IMusicAssistantCredentials Credentials { get; set; }

    public string LastSelectedPlayerId { get; set; }

    public VolumeEventControl VolumeEventControl { get; set; }
    public string LastSearchTerm { get; set; }
    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }
    public double WindowPosX { get; set; }
    public double WindowPosY { get; set; }
    LoggerSettings LoggerSettings { get; set; }

    public void Save();
}
