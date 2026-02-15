using Material.Icons;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using WateryTart.Core;
using WateryTart.Core.ViewModels;

namespace WateryTart.Platform.Windows.ViewModels
{
    public partial class SimpleWasapiPlayerSettingsViewModel : ReactiveObject, IViewModelBase, IHaveSettings
    {
        public string? UrlPathSegment { get; } = "wasapisettings";
        public IScreen? HostScreen { get; } = null;
        public string Title { get; set; } = "wasapisettings";
        public bool ShowMiniPlayer { get; } = false;
        public bool ShowNavigation { get; } = false;
        public MaterialIconKind Icon => MaterialIconKind.Speaker;
        [Reactive] public partial bool IsLoading { get; set; } = false;
    }
}
