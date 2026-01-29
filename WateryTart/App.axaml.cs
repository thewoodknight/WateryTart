using AsyncImageLoader.Loaders;
using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Config.Net;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using WateryTart.MassClient;
using WateryTart.Services;
using WateryTart.Settings;
using WateryTart.ViewModels;
using WateryTart.ViewModels.Players;


namespace WateryTart;

public static class AntipatternExtensionsYesIKnowItsBad
{
    public static T GetRequiredService<T>(this IContainer c)
    {
        return c.Resolve<T>();
    }
}
public partial class App : Application
{
    public static IContainer Container;
    public static string BaseUrl => Container.GetRequiredService<ISettings>().Credentials.BaseUrl;

    public static DiskCachedWebImageLoader ImageLoaderInstance { get; } = new DiskCachedWebImageLoader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Library", "WateryTart"));

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var builder = new ContainerBuilder();

        //Services
        builder.RegisterType<MainWindowViewModel>().As<IScreen>().SingleInstance();
        builder.RegisterType<MassWsClient>().As<IMassWsClient>().SingleInstance();
        builder.RegisterType<PlayersService>().As<IPlayersService>().SingleInstance();

        //Settings
        var settings = new ConfigurationBuilder<ISettings>().UseJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Library", "WateryTart")).Build();
        builder.RegisterInstance<ISettings>(settings).SingleInstance();

        //View models that are singleton
        builder.RegisterType<SettingsViewModel>().SingleInstance();
        builder.RegisterType<PlayersViewModel>().SingleInstance();
        builder.RegisterType<MiniPlayerViewModel>().SingleInstance();
        builder.RegisterType<BigPlayerViewModel>().SingleInstance();
        builder.RegisterType<HomeViewModel>().SingleInstance();

        //Volume controllers
        builder.RegisterType<WindowsVolumeService>().AsImplementedInterfaces().SingleInstance();
#if ARMRELEASE
        builder.RegisterType<GpioVolumeService>().AsImplementedInterfaces().SingleInstance();
#endif

        //Transient viewmodels
        builder.RegisterType<AlbumsListViewModel>();
        builder.RegisterType<AlbumViewModel>();
        builder.RegisterType<LoginViewModel>();
        builder.RegisterType<PlaylistViewModel>();
        builder.RegisterType<ArtistViewModel>();
        builder.RegisterType<SearchViewModel>();
        builder.RegisterType<ArtistsViewModel>();
        builder.RegisterType<LibraryViewModel>();
        builder.RegisterType<RecommendationViewModel>().AsImplementedInterfaces(); ;

        Container = builder.Build();

        var vm = Container.GetRequiredService<IScreen>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };

            //Shutdown
            ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime).ShutdownRequested += (s, e) =>
            {
                var reapers = Container.Resolve<IEnumerable<IReaper>>();
                foreach (var reaper in reapers)
                {
                    reaper.Reap();
                }
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = vm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

}