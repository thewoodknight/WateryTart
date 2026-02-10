using Avalonia;
using ReactiveUI.Avalonia;
using System;
using System.Reflection;
using WateryTart.Core;
using WateryTart.Core.Playback;
using WateryTart.Platform.Windows.ViewModels;
using WateryTart.Platform.Windows.Views;

namespace WateryTart.Platform.Windows;
    
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
            // Register platform-specific views BEFORE creating App
            ViewLocator.RegisterView<SimpleWasapiPlayerSettingsViewModel, SimpleWasapiPlayerSettingsView>();
            
            var x = new App(
            [
                new InstancePlatformSpecificRegistration<IPlayerFactory>(new WindowsAudioPlayerFactory()),
                new LambdaRegistration<SimpleWasapiPlayerSettingsViewModel>(
                    c => new SimpleWasapiPlayerSettingsViewModel(/* pass dependencies */)
                )
            ]);
            return x;
        })
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
