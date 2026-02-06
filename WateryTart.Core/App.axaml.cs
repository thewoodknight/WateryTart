using AsyncImageLoader.Loaders;
using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using WateryTart.Core.Playback;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels;
using WateryTart.Core.ViewModels.Players;
using WateryTart.Core.Views;
using WateryTart.Service.MassClient;
using ColourService = WateryTart.Core.Services.ColourService;
using PlayersService = WateryTart.Core.Services.PlayersService;

namespace WateryTart.Core;

public static class AntipatternExtensionsYesIKnowItsBad
{
    public static T GetRequiredService<T>(this IContainer c)
    {
        return c.Resolve<T>();
    }
}

public partial class App : Application
{

    private static readonly string baseAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WateryTart");
    private static readonly string AppDataPath = Path.Combine(baseAppDataPath, "Cache");
    private static readonly Lazy<DiskCachedWebImageLoader> LazyImageLoader = new(() => new DiskCachedWebImageLoader(AppDataPath));
    private static string _cachedBaseUrl;
    private static IEnumerable<IReaper> _reapers;
    private static bool _isShuttingDown;

    public static IContainer Container;
    public static string BaseUrl
    {
        get
        {
            if (string.IsNullOrEmpty(_cachedBaseUrl))
            {
                _cachedBaseUrl = Container.Resolve<ISettings>().Credentials.BaseUrl;
            }
            return _cachedBaseUrl;
        }
    }

    private static Bitmap fallbackImage;
    public static Bitmap FallbackImage
    {
        get
        {
            return fallbackImage ??= new Bitmap(AssetLoader.Open(new Uri("avares://WateryTart.Core/Assets/cover_dark.png")));
        }
    }
    public static DiskCachedWebImageLoader ImageLoaderInstance => LazyImageLoader.Value;

    private IEnumerable<IPlatformSpecificRegistration> PlatformSpecificRegistrations { get; }

    public App(IEnumerable<IPlatformSpecificRegistration> platformSpecificRegistrations)
    {
        PlatformSpecificRegistrations = platformSpecificRegistrations;
    }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var builder = new ContainerBuilder();

        //Services
        /* Explicit interface definitions so .AsImplemented() isn't call, which isn't AOT compatible */
        builder.Register(c => new MainWindowViewModel(
            c.Resolve<IMassWsClient>(),
            c.Resolve<IPlayersService>(),
            c.Resolve<ISettings>(),
            c.Resolve<IColourService>(),
            c.Resolve<SendSpinClient>()
        )).As<IScreen>().As<IActivatableViewModel>().SingleInstance();
        builder.RegisterType<MassWsClient>().As<IMassWsClient>().SingleInstance();
        builder.RegisterType<PlayersService>().As<IPlayersService>().As<IAsyncReaper>().SingleInstance();
        builder.RegisterType<ColourService>().As<IColourService>().SingleInstance();

        //Settings
        var x = Path.Combine(baseAppDataPath, "settings.json");
        var settings = new Settings.Settings(x);
        builder.RegisterInstance(settings).As<ISettings>().SingleInstance();

        //View models that are singleton
        builder.RegisterType<SettingsViewModel>().SingleInstance();
        builder.RegisterType<ServerSettingsViewModel>().As<IHaveSettings>().SingleInstance();
        builder.RegisterType<PlayersViewModel>().SingleInstance();
        builder.RegisterType<MiniPlayerViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<BigPlayerViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<HomeViewModel>().SingleInstance();
        builder.RegisterType<KeyboardVolumeKeyBindingsViewModel>().As<IHaveSettings>().SingleInstance();
        builder.RegisterType<SearchResultsViewModel>().AsSelf().SingleInstance();

        //Platform specific registrations from Platform.Linux, Platform.Windows projects
        foreach (var platformSpecificRegistration in PlatformSpecificRegistrations)
        {
            platformSpecificRegistration.Register(builder);
        }

        builder.RegisterType<SendSpinClient>().SingleInstance();

        //Volume controllers
        builder.RegisterType<WindowsVolumeService>().As<IVolumeService>().As<IReaper>().SingleInstance();

        //Transient viewmodels
        builder.RegisterType<AlbumsListViewModel>();
        builder.RegisterType<AlbumViewModel>();
        builder.RegisterType<LoginViewModel>();
        builder.RegisterType<PlaylistViewModel>();
        builder.RegisterType<ArtistViewModel>();
        builder.RegisterType<SearchViewModel>();
        builder.RegisterType<ArtistsViewModel>();
        builder.RegisterType<LibraryViewModel>();
        builder.RegisterType<TrackViewModel>();
        builder.RegisterType<RecommendationViewModel>();
        builder.RegisterType<PlaylistsViewModel>();
        builder.RegisterType<TracksViewModel>();

        Container = builder.Build();

        // Cache BaseUrl immediately after container is built
        _cachedBaseUrl = Container.Resolve<ISettings>().Credentials.BaseUrl;

        // Cache ALL reapers - including from singleton ViewModels
        _reapers = Container.Resolve<IEnumerable<IReaper>>();

        var vm = Container.Resolve<IScreen>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Views.MainWindow
            {
                DataContext = vm
            };

            desktop.ShutdownRequested += (s, e) =>
            {
                // Prevent re-entry - if we're already shutting down, let it proceed
                if (_isShuttingDown)
                {
                    Debug.WriteLine("=== SHUTDOWN PROCEEDING (cleanup already done) ===");
                    return;
                }

                Debug.WriteLine("=== SHUTDOWN STARTED ===");
                _isShuttingDown = true;
                e.Cancel = true;

                foreach (var reaper in _reapers)
                {
                    try
                    {
                        Debug.WriteLine($"Reaping {reaper.GetType().Name}");
                        reaper.Reap();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error reaping {reaper.GetType().Name}: {ex}");
                    }
                }

                LazyImageLoader.Value?.Dispose();

                System.Threading.Thread.Sleep(2000);

                Debug.WriteLine("=== REQUESTING SHUTDOWN ===");

                var process = System.Diagnostics.Process.GetCurrentProcess();
                Debug.WriteLine($"Total OS threads: {process.Threads.Count}");

                // Sample first 10 threads
                int count = 0;
                foreach (ProcessThread thread in process.Threads)
                {
                    if (count < 10)
                    {
                        Debug.WriteLine($"  Thread {thread.Id}: State={thread.ThreadState}");
                        count++;
                    }
                }

                // If too many threads, force exit instead of normal shutdown
                if (process.Threads.Count > 50)
                {
                    Debug.WriteLine($"Too many threads ({process.Threads.Count}) still running, forcing exit...");
                    Environment.Exit(0);
                }
                else
                {
                    desktop.Shutdown(0);
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