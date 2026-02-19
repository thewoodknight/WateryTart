using Iot.Device.RotaryEncoder;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
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

    // Rx debounce subscription
    private IDisposable? _pulseSubscription;    
    private const int DebounceIntervalMs = 100;
    private const int MaxStepsPerDebounce = 20;

    public GpioVolumeService(ISettings settings, PlayersService playersService)
    {
        this.playersService = playersService;
        rotaryEncoder = new QuadratureRotaryEncoder(PinB, PinA, PulsesPerTurn);
        oldValue = rotaryEncoder.PulseCount;
    
        // Create an observable from the rotary encoder event, debounce it,
        // then process the coalesced value on a background scheduler.
        _pulseSubscription = Observable
            .FromEventPattern<RotaryEncoderEventArgs>(
                h => rotaryEncoder.PulseCountChanged += h,
                h => rotaryEncoder.PulseCountChanged -= h)
            .Select(ep => ep.EventArgs.Value)
            .Throttle(TimeSpan.FromMilliseconds(DebounceIntervalMs))
            .ObserveOn(TaskPoolScheduler.Default)
            // subscribe with an async handler so we await PlayersService coordination
            .Subscribe(async pending => await OnDebouncedPulseAsync(pending));
    }

    // renamed and async
    private async Task OnDebouncedPulseAsync(double pending)
    {
        var delta = pending - oldValue;
        if (delta == 0)
            return;

        var steps = (int)Math.Abs(Math.Round(delta));
        if (steps <= 0)
            return;

        if (steps > MaxStepsPerDebounce)
            steps = MaxStepsPerDebounce;

        // Use the coordinated delta method on PlayersService; sign mapping preserved:
        if (delta < 0)
        {
            // encoder decrease -> volume up (original behaviour)
            await playersService.PlayerChangeBy(steps).ConfigureAwait(false);
        }
        else
        {
            await playersService.PlayerChangeBy(-steps).ConfigureAwait(false);
        }

        oldValue = pending;
    }

    public void Reap()
    {
        if (rotaryEncoder != null)
            rotaryEncoder.Dispose();

        _pulseSubscription?.Dispose();
        _pulseSubscription = null;
    }

    [Reactive] public partial bool IsEnabled { get; set; }
}
