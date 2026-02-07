using ReactiveUI;
using WateryTart.Core;
using WateryTart.Core.ViewModels;

namespace WateryTart.Platform.Windows.ViewModels
{
    public class SimpleWasapiPlayerSettingsViewModel : ReactiveObject, IViewModelBase, IHaveSettings
    {
        public string? UrlPathSegment { get; } = "wasapisettings";
        public IScreen? HostScreen { get; } = null;
        public string Title { get; set; } = "wasapisettings";
        public bool ShowMiniPlayer { get; } = false;
        public bool ShowNavigation { get; } = false;
        public string Icon => "Speaker";
    }
}
