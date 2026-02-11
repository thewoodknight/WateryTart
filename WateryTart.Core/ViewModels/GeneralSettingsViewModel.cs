using Material.Icons;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using WateryTart.Core.Settings;

namespace WateryTart.Core.ViewModels
{
    public partial class GeneralSettingsViewModel : ReactiveObject, IViewModelBase, IHaveSettings
    {
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
            //_selectedVolumeEvent = _settings.VolumeEventControl;

        }
    }
}