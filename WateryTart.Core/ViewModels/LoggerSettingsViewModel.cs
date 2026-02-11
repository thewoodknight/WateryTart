using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WateryTart.Core.Settings;

namespace WateryTart.Core.ViewModels;

public partial class LoggerSettingsViewModel : ReactiveObject, IViewModelBase, IHaveSettings
{
    private readonly ISettings _settings;

    public IEnumerable<LogLevel> AvailableLogLevels => new[]
    {
        LogLevel.Trace,
        LogLevel.Debug,
        LogLevel.Information,
        LogLevel.Warning,
        LogLevel.Error,
        LogLevel.Critical,
        LogLevel.None
    };

    public RelayCommand BrowseLogFileCommand { get; }
    [Reactive] public partial bool EnableFileLogging { get; set; }
    public IScreen HostScreen { get; }
    public MaterialIconKind Icon => MaterialIconKind.History;
    [Reactive] public partial bool IsSaving { get; set; }
    [Reactive] public partial string LogFilePath { get; set; }
    [Reactive] public partial string LogFileSize { get; set; }
    public RelayCommand OpenLogFolderCommand { get; }
    public AsyncRelayCommand SaveSettingsCommand { get; }
    [Reactive] public partial LogLevel SelectedLogLevel { get; set; }
    public bool ShowMiniPlayer { get; } = true;
    public bool ShowNavigation { get; } = true;
    [Reactive] public partial string StatusMessage { get; set; } = string.Empty;
    public string Title => "Logging";
    public string? UrlPathSegment { get; } = string.Empty;

    public LoggerSettingsViewModel(ISettings settings, IScreen screen)
    {
        _settings = settings;
        HostScreen = screen;

        // Load current settings
        SelectedLogLevel = _settings.LoggerSettings?.LogLevel ?? LogLevel.Information;
        EnableFileLogging = _settings.LoggerSettings?.EnableFileLogging ?? false;
        LogFilePath = _settings.LoggerSettings?.LogFilePath ?? GetDefaultLogPath();
        UpdateLogFileSize();

        BrowseLogFileCommand = new RelayCommand(BrowseLogFile);
        SaveSettingsCommand = new AsyncRelayCommand(SaveSettings);
        OpenLogFolderCommand = new RelayCommand(OpenLogFolder);
    }

    private void BrowseLogFile()
    {
        // You can use a file dialog here - for now we'll just show the folder
        OpenLogFolder();
    }

    private string GetDefaultLogPath()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WateryTart");
        return Path.Combine(appDataPath, "watertart.log");
    }

    private void OpenLogFolder()
    {
        try
        {
            var directory = Path.GetDirectoryName(LogFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = directory,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening folder: {ex.Message}";
        }
    }

    private async Task SaveSettings()
    {
        try
        {
            IsSaving = true;
            StatusMessage = "Saving...";

            // Ensure directory exists
            var directory = Path.GetDirectoryName(LogFilePath);
            if (!string.IsNullOrEmpty(directory) && EnableFileLogging)
            {
                Directory.CreateDirectory(directory);
            }

            // Update settings
            _settings.LoggerSettings.LogLevel = SelectedLogLevel;
            _settings.LoggerSettings.EnableFileLogging = EnableFileLogging;
            _settings.LoggerSettings.LogFilePath = LogFilePath;
            _settings.Save();

            await Task.Delay(500); // Brief delay for UX feedback
            StatusMessage = "Settings saved successfully";
            await Task.Delay(2000);
            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void UpdateLogFileSize()
    {
        try
        {
            if (File.Exists(LogFilePath))
            {
                var fileInfo = new FileInfo(LogFilePath);
                var sizeMb = fileInfo.Length / (1024.0 * 1024.0);
                LogFileSize = $"{sizeMb:F2} MB";
            }
            else
            {
                LogFileSize = "No log file yet";
            }
        }
        catch
        {
            LogFileSize = "Unable to read file size";
        }
    }
}