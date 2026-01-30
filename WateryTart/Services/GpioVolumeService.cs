using Iot.Device.RotaryEncoder;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using WateryTart.Settings;

namespace WateryTart.Services;

public partial class GpioVolumeService : ReactiveObject, IVolumeService, IReaper
{
    const int PinA = 17;
    const int PinB = 27;
    const int PulsesPerTurn = 20;
    private readonly IPlayersService playersService;
    double oldValue = 0;
    private QuadratureRotaryEncoder rotaryEncoder;

    [Reactive] public partial bool IsEnabled { get; set; }

    public GpioVolumeService(ISettings settings, IPlayersService playersService)
    {
        this.playersService = playersService;
        Iot.Device.RotaryEncoder.QuadratureRotaryEncoder rotaryEncoder = new QuadratureRotaryEncoder(PinA, PinB, PulsesPerTurn);
        oldValue = rotaryEncoder.PulseCount;
        rotaryEncoder.PulseCountChanged += PulseCountChanged;
    }

    private void PulseCountChanged(object? sender, RotaryEncoderEventArgs e)
    {
        if (e.Value < oldValue)
        {
            playersService.PlayerVolumeUp();
        }
        else if (e.Value > oldValue)
        {
            playersService.PlayerVolumeDown();
        }
        oldValue = e.Value;
    }

    public void Reap()
    {
        rotaryEncoder.Dispose();
    }
}
