using Autofac;
using Avalonia;
using Microsoft.Extensions.Logging;
using ReactiveUI.Avalonia;
using System;
using System.Reflection;
using Velopack;
using WateryTart.Core;
using WateryTart.Core.Playback;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.Platform.Windows.Playback;
using WateryTart.Platform.Windows.ViewModels;
using WateryTart.Platform.Windows.Views;

namespace WateryTart.Platform.Windows;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>(() =>
        {
            // Register platform-specific views BEFORE creating App
            ViewLocator.RegisterView<SimpleWasapiPlayerSettingsViewModel, SimpleWasapiPlayerSettingsView>();

            ViewLocator.RegisterView<PlaybackSettingsViewModel, PlaybackSettingsView>();

            var x = new App(
            [
                new InstancePlatformSpecificRegistration<Playback.SwitchableAudioPlayer>(new Playback.SwitchableAudioPlayer(
                    () => new SimpleWasapiPlayer(),
                    () => new SoundflowPlayer(),
                    Playback.SwitchableAudioPlayer.PlayerBackend.SimpleWasapi
                )),

                new LambdaRegistration<IPlayerFactory>(c => new WindowsAudioPlayerFactory(() => App.Container.Resolve<Playback.SwitchableAudioPlayer>())),
                new LambdaRegistration<IVolumeService>(c => new WindowsVolumeService(c.Resolve<PlayersService>())),
                new LambdaRegistration<IHaveSettings>(c => new PlaybackSettingsViewModel(c.Resolve<ISettings>(), c.Resolve<Playback.SwitchableAudioPlayer>(), c.Resolve<ILoggerFactory>())),
            ]);
            return x;
        })
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
