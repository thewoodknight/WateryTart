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
    private readonly PlayersService playersService;
    private double oldValue = 0;
    private QuadratureRotaryEncoder? rotaryEncoder;

    // Rx debounce subscription
    private IDisposable? _pulseSubscription;

    private const int DebounceIntervalMs = 100;
    private const int MaxStepsPerDebounce = 20;

    private readonly object _oldValueLock = new();
    private ISettings _settings;

    public GpioVolumeService(ISettings settings, PlayersService playersService)
    {
#if !LINUX_ARM64
        return;
#endif

        _settings = settings;
        this.playersService = playersService;
        // Ensure pin order matches the QuadratureRotaryEncoder constructor (pinA, pinB)

        if (settings.CustomSettings.ContainsKey("GpioEnable") &&
    settings.CustomSettings["GpioEnable"] is bool enabled &&
    !enabled)
        {
            return;
        }

        Enable();
    }

    private void Enable()
    {
        var PinA = (int)_settings.CustomSettings["GpioPinA"];
        var PinB = (int)_settings.CustomSettings["GpioPinB"];
        var PulsesPerTurn = (int)_settings.CustomSettings["GpioPulsesPerTurn"];

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

    public void SetEnable(bool status)
    {
        if (status)
        {
            if (rotaryEncoder == null)
            {
                Enable();
            }
        }
        else
        {
            Reap();
        }
    }

    [Reactive] public partial bool IsEnabled { get; set; }
}