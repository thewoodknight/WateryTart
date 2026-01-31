using Avalonia;
using ReactiveUI.Avalonia;
using System;
using WateryTart.Core;
using WateryTart.Core.Playback;

namespace WateryTart.Platform.Linux;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>(() =>
        {
            var x = new App(
                [
                    new InstancePlatformSpecificRegistration(new LinuxAudioPlayerFactory()),
                    new TypePlatformSpecificRegistration<GpioVolumeService>(),
                ]);

            return x;
        })
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .WithDeveloperTools()
            .LogToTrace();
}
