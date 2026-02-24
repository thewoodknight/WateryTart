using IconPacks.Avalonia.Material;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using WateryTart.Core.Extensions;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels;

namespace WateryTart.Platform.Linux.ViewModels;

public partial class GpioVolumeSettingsViewModel : ReactiveObject, IViewModelBase, IHaveSettings
{
    private bool _isEnabled;
    private int _pinA = 17;
    private int _pinB = 27;
    private int _pulsesPerTurn = 20;
    private ISettings _settings;
    public IScreen HostScreen { get; }
    public PackIconMaterialKind Icon => PackIconMaterialKind.DeveloperBoard;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _isEnabled, value);
            _settings.Save();
            _gpioVolumeService.SetEnable(value);
        }
    }

    [Reactive] public partial bool IsLoading { get; set; } = false;

    public int PinA
    {
        get => _pinA;
        set
        {
            this.RaiseAndSetIfChanged(ref _pinA, value);
            _settings.Save();
        }
    }

    public int PinB
    {
        get => _pinB;
        set
        {
            this.RaiseAndSetIfChanged(ref _pinB, value);
            _settings.Save();
        }
    }

    public int PulsesPerTurn
    {
        get => _pulsesPerTurn;
        set
        {
            this.RaiseAndSetIfChanged(ref _pulsesPerTurn, value);
            _settings.Save();
        }
    }

    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => false;
    public string Title { get; set; } = "GPIO Settings";
    public string? UrlPathSegment => null;

    private GpioVolumeService _gpioVolumeService;
    public GpioVolumeSettingsViewModel(ISettings settings, IScreen hostScreen, GpioVolumeService gpioVolumeService)
    {
        _gpioVolumeService = gpioVolumeService;
        _settings = settings;
        HostScreen = hostScreen;
        IsEnabled = settings.CustomSettings.ContainsKey("GpioEnable") &&
            settings.CustomSettings["GpioEnable"] is bool enabled &&
            enabled;

        var gpioA = settings.CustomSettings.TryGet<int>("GpioPinA");
        PinA = gpioA != null ? (int)gpioA : 17;

        var gpioB = settings.CustomSettings.TryGet<int>("GpioPinB");
        PinB = gpioB != null ? (int)gpioB : 27;

        var GpioPulsesPerTurn = settings.CustomSettings.TryGet<int>("GpioPulsesPerTurn");
        PulsesPerTurn = GpioPulsesPerTurn != null ? (int)GpioPulsesPerTurn : 20;
    }
}