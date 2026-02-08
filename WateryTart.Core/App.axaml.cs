using AsyncImageLoader.Loaders;
using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

    private static readonly string BaseAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WateryTart");
    private static readonly string AppDataPath = Path.Combine(BaseAppDataPath, "Cache");
    private static readonly Lazy<DiskCachedWebImageLoader> LazyImageLoader = new(() => new DiskCachedWebImageLoader(AppDataPath));
    private static string? _cachedBaseUrl;
    private static IEnumerable<IReaper>? _reapers;
    private static bool _isShuttingDown;
    private static ILoggerFactory? _loggerFactory;
    private static ILogger? _logger;

    public static IContainer Container;
    
    public static ILogger? Logger => _logger;
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

    private static Bitmap? fallbackImage;
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

        //Settings - Load first before creating logger
        var settingsPath = Path.Combine(BaseAppDataPath, "settings.json");
        var settings = new Settings.Settings(settingsPath);
        builder.RegisterInstance(settings).As<ISettings>().SingleInstance();

        // Initialize logger factory with settings
        var logLevel = settings.LoggerSettings?.LogLevel ?? LogLevel.Information;
        
        _loggerFactory = LoggerFactory.Create(logBuilder =>
        {
            logBuilder.SetMinimumLevel(logLevel).AddConsole();
            
            // Add file logging if enabled
            if (settings.LoggerSettings?.EnableFileLogging ?? false)
            {
                var logPath = settings.LoggerSettings.LogFilePath;
                if (!string.IsNullOrEmpty(logPath))
                {
                    var directory = Path.GetDirectoryName(logPath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Uncomment when you have file logging provider
                    logBuilder.AddFile(o =>
                    {
                        
                        o.RootPath = logPath;
                       // o.BasePath = "WateryTart.Logs";
                        o.MaxFileSize = 10_000_000;
                        o.FileAccessMode = Karambolo.Extensions.Logging.File.LogFileAccessMode.KeepOpenAndAutoFlush;
                        o.Files = new[]
                        {
                            new Karambolo.Extensions.Logging.File.LogFileOptions { Path = "default.log" }
                        };
                    });
                }
            }
        });
        
        _logger = _loggerFactory.CreateLogger("WateryTart.Core");

        // Register the logger factory for DI
        builder.RegisterInstance(_loggerFactory).As<ILoggerFactory>().SingleInstance();

        //Services
        /* Explicit interface definitions so .AsImplemented() isn't call, which isn't AOT compatible */
        builder.Register(c => new MainWindowViewModel(
            c.Resolve<IMassWsClient>(),
            c.Resolve<IPlayersService>(),
            c.Resolve<ISettings>(),
            c.Resolve<IColourService>(),
            c.Resolve<SendSpinClient>(),
            c.Resolve<ILoggerFactory>()
        )).As<IScreen>().As<IActivatableViewModel>().SingleInstance();
        builder.RegisterType<MassWsClient>().As<IMassWsClient>().SingleInstance();
        builder.RegisterType<PlayersService>().As<IPlayersService>().As<IAsyncReaper>().SingleInstance();
        builder.RegisterType<ColourService>().As<IColourService>().SingleInstance();

        //View models that are singleton
        builder.RegisterType<SettingsViewModel>().SingleInstance();
        builder.RegisterType<ServerSettingsViewModel>().As<IHaveSettings>().SingleInstance();
        builder.RegisterType<PlayersViewModel>().SingleInstance();
        builder.RegisterType<MiniPlayerViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<BigPlayerViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<HomeViewModel>().SingleInstance();
        builder.RegisterType<KeyboardVolumeKeyBindingsViewModel>().As<IHaveSettings>().SingleInstance();
        builder.RegisterType<SearchResultsViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<LoggerSettingsViewModel>().As<IHaveSettings>().SingleInstance();
        builder.RegisterType<SearchViewModel>().SingleInstance();

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
        builder.RegisterType<ArtistsViewModel>();
        builder.RegisterType<LibraryViewModel>();
        builder.RegisterType<TrackViewModel>();
        builder.RegisterType<RecommendationViewModel>();
        builder.RegisterType<PlaylistsViewModel>();
        builder.RegisterType<TracksViewModel>();
        builder.RegisterType<SimilarTracksViewModel>();
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
                    _logger?.LogInformation("=== SHUTDOWN PROCEEDING (cleanup already done) ===");
                    return;
                }

                _logger?.LogInformation("=== SHUTDOWN STARTED ===");
                _isShuttingDown = true;
                e.Cancel = true;

                foreach (var reaper in _reapers ?? [])
                {
                    try
                    {
                        _logger?.LogInformation("Reaping {ReaperType}", reaper.GetType().Name);
                        reaper.Reap();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error reaping {ReaperType}", reaper.GetType().Name);
                    }
                }

                LazyImageLoader.Value?.Dispose();

                System.Threading.Thread.Sleep(2000);

                _logger?.LogInformation("=== REQUESTING SHUTDOWN ===");

                var process = System.Diagnostics.Process.GetCurrentProcess();
                _logger?.LogInformation("Total OS threads: {ThreadCount}", process.Threads.Count);

                // Sample first 10 threads
                int count = 0;
                foreach (ProcessThread thread in process.Threads)
                {
                    if (count < 10)
                    {
                        _logger?.LogDebug("Thread {ThreadId}: State={ThreadState}", thread.Id, thread.ThreadState);
                        count++;
                    }
                }

                // If too many threads, force exit instead of normal shutdown
                if (process.Threads.Count > 50)
                {
                    _logger?.LogWarning("Too many threads ({ThreadCount}) still running, forcing exit...", process.Threads.Count);
                    Environment.Exit(0);
                }
                else
                {
                    _loggerFactory?.Dispose();
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