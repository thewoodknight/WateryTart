using Iot.Device.RotaryEncoder;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;

namespace WateryTart.Platform.Linux;

public partial class GpioVolumeService : ReactiveObject, IVolumeService, IReaper
{
    const int PinA = 17;
    const int PinB = 27;
    const int PulsesPerTurn = 20;
    private readonly PlayersService playersService;
    double oldValue = 0;
    private QuadratureRotaryEncoder? rotaryEncoder;

    public GpioVolumeService(ISettings settings, PlayersService playersService)
    {
        this.playersService = playersService;
        rotaryEncoder = new QuadratureRotaryEncoder(PinA, PinB, PulsesPerTurn);
        oldValue = rotaryEncoder.PulseCount;
        rotaryEncoder.PulseCountChanged += PulseCountChanged;
    }

    private void PulseCountChanged(object? sender, RotaryEncoderEventArgs e)
    {
        if (playersService == null)
            return;

        if (e.Value < oldValue)
        {
#pragma warning disable CS4014
            playersService.PlayerVolumeUp();
        }
        else if (e.Value > oldValue)
        {
            playersService.PlayerVolumeDown();
#pragma warning restore CS4014
        }
        oldValue = e.Value;
    }

    public void Reap()
    {
        if (rotaryEncoder != null)
            rotaryEncoder.Dispose();
    }

    [Reactive] public partial bool IsEnabled { get; set; }
}
