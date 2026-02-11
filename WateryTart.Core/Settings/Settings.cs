using Avalonia.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using WateryTart.MusicAssistant.Models.Auth;

namespace WateryTart.Core.Settings;

public enum VolumeEventControl
{
    [Description("System Volume")]
    SystemVolume,
    [Description("App Volume")]
    AppVolume
}

public partial class Settings : INotifyPropertyChanged, ISettings
{
    [JsonConverter(typeof(MusicAssistantCredentialsConverter))]
    public IMusicAssistantCredentials Credentials
    {
        get => field ?? new MusicAssistantCredentials();
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public VolumeEventControl VolumeEventControl
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public string LastSelectedPlayerId
    {
        get => field ?? string.Empty;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public string LastSearchTerm
    {
        get => field ?? string.Empty;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public double WindowWidth
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public double WindowHeight
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public double WindowPosX
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public double WindowPosY
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    public LoggerSettings LoggerSettings
    {
        get => field;
        set
        {
            field = value;
            NotifyPropertyChanged();
            Save();
        }
    }

    [JsonIgnore]
    public string Path
    {
        get => field ?? string.Empty;
        set
        {
            field = value;
            NotifyPropertyChanged();
        }
    }

    private bool _suppressSave = true;

    public Settings(string path)
    {
        Credentials = new MusicAssistantCredentials();
        Path = path;
        if (!string.IsNullOrEmpty(path))
            Load(path);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Load(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                var fileData = File.ReadAllText(path);

                var loaded = JsonSerializer.Deserialize<Settings>(fileData, SettingsJsonContext.Default.Settings);

                if (loaded != null)
                {
                    Credentials = loaded.Credentials ?? new MusicAssistantCredentials();
                    LastSelectedPlayerId = loaded.LastSelectedPlayerId ?? string.Empty;
                    LastSearchTerm = loaded.LastSearchTerm ?? string.Empty;
                    WindowWidth = loaded.WindowWidth;
                    WindowHeight = loaded.WindowHeight;
                    WindowPosX = loaded.WindowPosX;
                    WindowPosY = loaded.WindowPosY;
                    LoggerSettings = loaded.LoggerSettings;
                    VolumeEventControl = loaded.VolumeEventControl;
                }

                // Initialize if not loaded
                LoggerSettings ??= new LoggerSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }


        }
        else
        {
            var fi = new FileInfo(path);
            if (!fi.Directory?.Exists ?? false)
                fi.Directory?.Create();
        }

        _suppressSave = false;
    }

    public void Save()
    {
        if (!_suppressSave && !string.IsNullOrEmpty(Path))
        {
            try
            {
                var json = JsonSerializer.Serialize(this, SettingsJsonContext.Default.Settings);
                File.WriteAllText(Path, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// AOT-compatible JSON source generator context for Settings serialization.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(Settings))]
[JsonSerializable(typeof(MusicAssistantCredentials))]
[JsonSerializable(typeof(LoggerSettings))]
internal partial class SettingsJsonContext : JsonSerializerContext
{
}