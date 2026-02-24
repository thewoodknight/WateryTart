using IconPacks.Avalonia.Material;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Velopack;
using Velopack.Sources;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;

namespace WateryTart.Core.ViewModels
{
    public partial class GeneralSettingsViewModel : ReactiveObject, IViewModelBase, IHaveSettings
    {
        private readonly ILogger _logger;
        private readonly ISettings _settings;
        private ITrayService _trayService;
        private UpdateManager _um;
        private UpdateInfo _update = default!;
        public IScreen HostScreen => null!;
        public PackIconMaterialKind Icon => PackIconMaterialKind.Cog;
        [Reactive] public partial string InstalledVersion { get; set; }
        [Reactive] public partial bool IsLoading { get; set; } = false;
        [Reactive] public partial string LatestVersion { get; set; } = string.Empty;

        public VolumeEventControl SelectedVolumeEvent
        {
            get => _settings.VolumeEventControl;
            set
            {
                if (_settings.VolumeEventControl != value)
                    _settings.VolumeEventControl = value;
            }
        }

        public bool ShowMiniPlayer => false;
        public bool ShowNavigation => false;
        public string Title => string.Empty;
        [Reactive] public partial bool TrayIcon { get; set; } = false;
        public ICommand TrayIconCommand { get; set; }
        public string? UrlPathSegment => string.Empty;
        public IEnumerable<VolumeEventControl> VolumeEventOptions { get; } = (VolumeEventControl[])Enum.GetValues(typeof(VolumeEventControl));

        public GeneralSettingsViewModel(ISettings settings, ILoggerFactory loggerFactory, ITrayService trayService)
        {
            _trayService = trayService;
            _settings = settings;
            TrayIcon = _settings.TrayIcon;
            _logger = loggerFactory.CreateLogger<GeneralSettingsViewModel>();
            _um = new UpdateManager(new GithubSource("https://github.com/TemuWolverine/WateryTart/", null, false));

            TrayIconCommand = ReactiveCommand.Create(() =>
            {
                _settings.TrayIcon = TrayIcon;

                if (_settings.TrayIcon)
                    _trayService.CreateTrayIcon();
                else
                    _trayService.Dispose();                
            });

            try
            {
                CheckForUpdates();

                InstalledVersion = _um.IsInstalled ? _um.CurrentVersion.ToString() : "(n/a - not installed)";
            }
            catch (Exception ex)
            {
                LatestVersion = $"Error checking for updates: {ex.Message}";
                InstalledVersion = "(n/a - not installed)";
                _logger.LogError(ex, "Failed to initialize UpdateManager");
            }
        }

        public async Task CheckForUpdates()
        {
            try
            {
                if (_um.IsInstalled)
                {
                    _update = await _um.CheckForUpdatesAsync().ConfigureAwait(true);
                    LatestVersion = _update.TargetFullRelease.Version.ToString();
                }
                else LatestVersion = "(n/a - not installed)";
            }
            catch (Exception ex)
            {
                LatestVersion = $"Error checking for updates: {ex.Message}";
                _logger.LogError(ex, "Failed to initialize UpdateManager");
            }
        }
    }
}