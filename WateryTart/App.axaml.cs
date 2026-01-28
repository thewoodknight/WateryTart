using AsyncImageLoader.Loaders;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Config.Net;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using System;
using System.IO;
using System.Reflection;
using WateryTart.MassClient;
using WateryTart.Services;
using WateryTart.Settings;
using WateryTart.ViewModels;
using WateryTart.ViewModels.Players;


namespace WateryTart;

public partial class App : Application
{
    public static ServiceProvider Container;
    public static string BaseUrl => Container.GetRequiredService<ISettings>().Credentials.BaseUrl;

    public static DiskCachedWebImageLoader ImageLoaderInstance { get; } = new DiskCachedWebImageLoader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Library", "WateryTart"));

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Register all the services needed for the application to run
        var collection = new ServiceCollection();

        collection.AddSingleton<IMassWsClient, MassWsClient>();
        collection.AddSingleton<IScreen, MainWindowViewModel>();
        collection.AddSingleton<IPlayersService, PlayersService>();
        collection.AddSingleton<SettingsViewModel>();
        collection.AddSingleton<PlayersViewModel>();
        collection.AddSingleton<MiniPlayerViewModel>();
        collection.AddSingleton<BigPlayerViewModel>();

        collection.AddTransient<AlbumsListViewModel>();
        collection.AddTransient<AlbumViewModel>();
        collection.AddTransient<LoginViewModel>();
        collection.AddTransient<HomeViewModel>();
        collection.AddTransient<PlaylistViewModel>();
        collection.AddTransient<ArtistViewModel>();
        collection.AddTransient<SearchViewModel>();
        collection.AddTransient<ArtistsViewModel>();
        collection.AddTransient<LibraryViewModel>();
        collection.AddTransient<RecommendationViewModel>();

        AppLocator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
        //settings
        var settings = new ConfigurationBuilder<ISettings>()
               .UseJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Library", "WateryTart"))
            .Build();

        collection.AddSingleton<ISettings>(settings);

        // Creates a ServiceProvider containing services from the provided IServiceCollection
        Container = collection.BuildServiceProvider();

        var vm = Container.GetRequiredService<IScreen>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {


            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
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