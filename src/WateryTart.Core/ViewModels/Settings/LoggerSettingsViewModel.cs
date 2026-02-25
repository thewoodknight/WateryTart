using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Material;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public AsyncRelayCommand BrowseLogFileCommand { get; }
    [Reactive] public partial bool EnableFileLogging { get; set; }
    public IScreen HostScreen { get; }
    public PackIconMaterialKind Icon => PackIconMaterialKind.History;
    [Reactive] public partial bool IsLoading { get; set; } = false;
    [Reactive] public partial bool IsSaving { get; set; }
    [Reactive] public partial string LogFilePath { get; set; }
    [Reactive] public partial string LogFileSize { get; set; }
    public RelayCommand OpenLogFolderCommand { get; }
    [Reactive] public partial LogLevel SelectedLogLevel { get; set; }
    public bool ShowMiniPlayer { get; } = true;
    public bool ShowNavigation { get; } = true;
    [Reactive] public partial string StatusMessage { get; set; } = string.Empty;
    public string Title => "Logging";
    public string Description => "Logging settings for debugging WateryTart";
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

        BrowseLogFileCommand = new AsyncRelayCommand(BrowseLogFile);
        OpenLogFolderCommand = new RelayCommand(OpenLogFolder);
    }

    private async Task BrowseLogFile()
    {
        var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(mainWindow);

        // Start async operation to open the dialog.
        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Log Folder",
            AllowMultiple = false,

        });

        if (folder == null || folder.Count == 0)
        {
            StatusMessage = "No folder selected.";
            return;
        }

        LogFilePath =  folder[0].Path.LocalPath;
        SaveSettings();
    }

    private string GetDefaultLogPath()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WateryTart");
        return Path.Combine(appDataPath, "waterytart.log");
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
            var filepath = Path.Combine(LogFilePath, "default.log");
            if (File.Exists(filepath))
            {
                var fileInfo = new FileInfo(filepath);
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

        SaveSettings();
    }
}