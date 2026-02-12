using Material.Icons;
using Microsoft.VisualBasic;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;
using WateryTart.Core.Settings;

namespace WateryTart.Core.ViewModels
{
    public partial class GeneralSettingsViewModel : ReactiveObject, IViewModelBase, IHaveSettings
    {
        private UpdateManager _um;
        private UpdateInfo _update;
        public IEnumerable<VolumeEventControl> VolumeEventOptions { get; } = (VolumeEventControl[])Enum.GetValues(typeof(VolumeEventControl));
        private readonly ISettings _settings;

        public VolumeEventControl SelectedVolumeEvent
        {
            get => _settings.VolumeEventControl;
            set
            {
                if (_settings.VolumeEventControl != value)
                    _settings.VolumeEventControl = value;
            }
        }

        public string Title => string.Empty;

        public bool ShowMiniPlayer => false;

        public bool ShowNavigation => false;

        public string? UrlPathSegment => string.Empty;

        public IScreen HostScreen => null;

        public MaterialIconKind Icon => MaterialIconKind.Cog;

        public GeneralSettingsViewModel(ISettings settings)
        {
            _settings = settings;
            _um = new UpdateManager(new GithubSource("https://github.com/TemuWolverine/WateryTart/", null, false));
            CheckForUpdates();

            InstalledVersion = _um.IsInstalled ? _um.CurrentVersion.ToString() : "(n/a - not installed)";
        }
        public async Task CheckForUpdates()
        {
            if (_um.IsInstalled)
            {
                _update = await _um.CheckForUpdatesAsync().ConfigureAwait(true);
                LatestVersion = _update.TargetFullRelease.Version.ToString();
            } else LatestVersion = "(n/a - not installed)";
        }

        [Reactive] public partial string LatestVersion { get; set; }

        [Reactive] public partial string InstalledVersion { get; set; }

    }
}