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

    private readonly object _oldValueLock = new();  
    
    public GpioVolumeService(ISettings settings, PlayersService playersService)
    {
#if !LINUX_ARM64
        return;
#endif
        this.playersService = playersService;
        // Ensure pin order matches the QuadratureRotaryEncoder constructor (pinA, pinB)
        rotaryEncoder = new QuadratureRotaryEncoder(PinA, PinB, PulsesPerTurn);
        oldValue = rotaryEncoder.PulseCount;

        // Create an observable from the rotary encoder event, debounce it,
        // then process the coalesced value on a background scheduler.
        // Use SelectMany + FromAsync so async work is tracked by Rx instead of
        // using an async-void lambda in Subscribe.
        _pulseSubscription = Observable
            .FromEventPattern<RotaryEncoderEventArgs>(
                h => rotaryEncoder.PulseCountChanged += h,
                h => rotaryEncoder.PulseCountChanged -= h)
            .Select(ep => ep.EventArgs.Value)
            .Throttle(TimeSpan.FromMilliseconds(DebounceIntervalMs))
            .ObserveOn(TaskPoolScheduler.Default)
            .SelectMany(pending => Observable.FromAsync(() => OnDebouncedPulseAsync(pending)))
            .Subscribe();
    }

    // renamed and async
    private async Task OnDebouncedPulseAsync(double pending)
    {
        try
        {
            double delta;
            lock (_oldValueLock)
            {
                delta = pending - oldValue;
            }

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

            lock (_oldValueLock)
            {
                oldValue = pending;
            }
        }
        catch (Exception ex)
        {
            // Prevent unobserved exceptions from terminating the Rx pipeline.
            // Consider logging if a logging facility is available.
            //Debug.WriteLine($"GpioVolumeService: error handling pulse: {ex}");
        }
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
