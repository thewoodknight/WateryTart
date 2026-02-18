using Material.Icons;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels;

namespace WateryTart.Platform.Windows.ViewModels
{
    public partial class SimpleWasapiPlayerSettingsViewModel : ReactiveObject, IViewModelBase, IHaveSettings
    {
        public string? UrlPathSegment { get; } = "wasapisettings";

#pragma warning disable CS8618
        public IScreen HostScreen { get; }
#pragma warning restore CS8618
        public string Title { get; set; } = "wasapisettings";
        public bool ShowMiniPlayer { get; } = false;
        public bool ShowNavigation { get; } = false;
        public MaterialIconKind Icon => MaterialIconKind.Speaker;
        [Reactive] public partial bool IsLoading { get; set; } = false;
    }
}
