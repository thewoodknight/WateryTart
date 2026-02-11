using Material.Icons;
using ReactiveUI;
using WateryTart.Core;
using WateryTart.Core.ViewModels;

namespace WateryTart.Platform.Linux.ViewModels;

public class GpioVolumeSettingsViewModel : ReactiveObject, IViewModelBase, IHaveSettings
{
    private bool _isEnabled;
    private int _pinA = 17;
    private int _pinB = 27;
    private int _pulsesPerTurn = 20;

    public string? UrlPathSegment => null;
    public IScreen HostScreen { get; }
    public string Title { get; set; } = "GPIO Settings";
    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => false;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }

    public int PinA
    {
        get => _pinA;
        set => this.RaiseAndSetIfChanged(ref _pinA, value);
    }

    public int PinB
    {
        get => _pinB;
        set => this.RaiseAndSetIfChanged(ref _pinB, value);
    }

    public int PulsesPerTurn
    {
        get => _pulsesPerTurn;
        set => this.RaiseAndSetIfChanged(ref _pulsesPerTurn, value);
    }

    public MaterialIconKind Icon => MaterialIconKind.DeveloperBoard;
}
        