using Autofac;
using Avalonia;
using ReactiveUI.Avalonia;
using System;
using System.Reflection;
using Velopack;
using WateryTart.Core;
using WateryTart.Core.Playback;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
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

            var x = new App(
            [
                new InstancePlatformSpecificRegistration<IPlayerFactory>(new WindowsAudioPlayerFactory()),
                  new LambdaRegistration<IVolumeService>(c => new WindowsVolumeService(c.Resolve<PlayersService>())),
                new LambdaRegistration<IHaveSettings>(c => new SimpleWasapiPlayerSettingsViewModel())
            ]);
            //builder.RegisterType<WindowsVolumeService>().As<IVolumeService>().As<IReaper>().SingleInstance();
            return x;
        })
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
