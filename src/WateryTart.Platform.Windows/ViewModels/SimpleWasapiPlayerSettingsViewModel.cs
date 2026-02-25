using IconPacks.Avalonia.Material;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels;

namespace WateryTart.Platform.Windows.ViewModels
{
    public partial class SimpleWasapiPlayerSettingsViewModel : ReactiveObject, IViewModelBase, IHaveSettings
    {
        public string? UrlPathSegment { get; } = "WASAPI Settings";

#pragma warning disable CS8618
        public IScreen HostScreen { get; }
#pragma warning restore CS8618
        public string Title { get; set; } = "WASAPI Settings";
        public string Description => "Currently unused settings for WASAPI output";
        public bool ShowMiniPlayer { get; } = false;
        public bool ShowNavigation { get; } = false;
        public PackIconMaterialKind Icon => PackIconMaterialKind.Speaker;
        [Reactive] public partial bool IsLoading { get; set; } = false;
    }
}
