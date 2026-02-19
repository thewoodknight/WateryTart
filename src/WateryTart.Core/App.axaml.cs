using AsyncImageLoader.Loaders;
using Autofac;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;
using WateryTart.Core.Playback;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.Core.Utilities;
using WateryTart.Core.ViewModels;
using WateryTart.Core.ViewModels.Players;
using WateryTart.Core.Views;
using WateryTart.MusicAssistant;
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
    public static IContainer Container;
    private static readonly string BaseAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WateryTart");
    private static readonly string AppDataPath = Path.Combine(BaseAppDataPath, "Cache");
    
    private static readonly Lazy<DiskCachedWebImageLoader> LazyImageLoader = new(() => new DiskCachedWebImageLoader(AppDataPath));
    private static string? _cachedBaseUrl;
    private static ISettings? _cachedSettings;
    private static bool _isShuttingDown;
    private static ILogger? _logger;
    private static ILoggerFactory? _loggerFactory;
    private static IEnumerable<IReaper>? _reapers;
    private static SingleInstanceLock? _singleInstanceLock;
    private static Bitmap? fallbackImage;

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

    public static Bitmap FallbackImage
    {
        get
        {
            return fallbackImage ??= new Bitmap(AssetLoader.Open(new Uri("avares://WateryTart.Core/Assets/cover_dark.png")));
        }
    }

    public static DiskCachedWebImageLoader ImageLoaderInstance => LazyImageLoader.Value;
    public static ILauncher Launcher { get; set; }
    public static ILogger? Logger => _logger;

    public static ISettings Settings
    {
        get
        {
            _cachedSettings ??= Container.Resolve<ISettings>();
            return _cachedSettings;
        }
    }

    private IEnumerable<IPlatformSpecificRegistration> PlatformSpecificRegistrations { get; }

    public App()
    {
    }

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
        // Try to acquire a filesystem lock to ensure single instance
        try
        {
            var lockPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WateryTart", "watertart.lock");
            if (!SingleInstanceLock.TryAcquire(lockPath, out _singleInstanceLock))
            {
                // Another instance is running - exit early
                Console.WriteLine("WateryTart is already running. Only one instance is allowed.");
                Environment.Exit(1);
                return;
            }
        }
        catch (Exception ex)
        {
            // If lock acquisition fails unexpectedly, log and continue — don't prevent startup for non-critical failures
            Console.WriteLine($"Warning acquiring single-instance lock: {ex.Message}");
        }
        
        UpdateMyApp();
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
            c.Resolve<MusicAssistantClient>(),
            c.Resolve<PlayersService>(),
            c.Resolve<ISettings>(),
            c.Resolve<ColourService>(),
            c.Resolve<SendSpinClient>(),
            c.Resolve<ILoggerFactory>(),
            c.Resolve<ProviderService>()
        )).As<IScreen>().As<IActivatableViewModel>().SingleInstance();
        builder.RegisterType<MusicAssistantClient>().As<MusicAssistantClient>().SingleInstance();
        builder.RegisterType<PlayersService>().As<PlayersService>().As<IAsyncReaper>().SingleInstance();
        builder.RegisterType<ColourService>().As<ColourService>().SingleInstance();

        //View models that are singleton
        builder.RegisterType<SettingsViewModel>().SingleInstance();
        builder.RegisterType<GeneralSettingsViewModel>().As<IHaveSettings>().SingleInstance();
        builder.RegisterType<ServerSettingsViewModel>().As<IHaveSettings>().SingleInstance();
        builder.RegisterType<PlayersViewModel>().SingleInstance();
        builder.RegisterType<MiniPlayerViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<BigPlayerViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<HomeViewModel>().SingleInstance();
        builder.RegisterType<KeyboardVolumeKeyBindingsViewModel>().As<IHaveSettings>().SingleInstance();
        builder.RegisterType<SearchResultsViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<LoggerSettingsViewModel>().As<IHaveSettings>().SingleInstance();
        builder.RegisterType<SearchViewModel>().SingleInstance();
        builder.RegisterType<ProviderService>().SingleInstance();

        //Platform specific registrations from Platform.Linux, Platform.Windows projects
        if (PlatformSpecificRegistrations != null)
            foreach (var platformSpecificRegistration in PlatformSpecificRegistrations)
            {
                platformSpecificRegistration.Register(builder);
            }


        builder.RegisterType<SendSpinClient>().SingleInstance();

        //Volume controllers
        

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

        var volumeProviders = Container.Resolve<IEnumerable<IVolumeService>>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var hostView = new Views.MainWindow();
            hostView.Host.DataContext = vm;
            desktop.MainWindow = hostView;

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
                    // dispose single-instance lock before forcing exit
                    _singleInstanceLock?.Dispose();
                    Environment.Exit(0);
                }
                else
                {
                    _loggerFactory?.Dispose();
                    // release single-instance lock before normal shutdown
                    _singleInstanceLock?.Dispose();
                    desktop.Shutdown(0);
                }
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var hostView = new MainSingleView();
            hostView.Host.DataContext = vm;
            singleViewPlatform.MainView = hostView;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task UpdateMyApp()
    {
        var gs = new GithubSource("https://github.com/TemuWolverine/WateryTart/", null, false);
        var mgr = new UpdateManager(gs);

        // check for new version
        try
        {
            var newVersion = await mgr.CheckForUpdatesAsync();
            if (newVersion == null)
                return; // no update available

            // download new version
            await mgr.DownloadUpdatesAsync(newVersion);

            // install new version and restart app
            mgr.ApplyUpdatesAndRestart(newVersion);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
}