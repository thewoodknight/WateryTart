using AsyncImageLoader.Loaders;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Config.Net;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using WateryTart.MassClient;
using WateryTart.Services;
using WateryTart.Settings;
using WateryTart.ViewModels;
using WateryTart.Views;

namespace WateryTart;

public partial class App : Application
{
    public static ServiceProvider Container;

    public static string BaseUrl => Container.GetRequiredService<ISettings>().Credentials.BaseUrl;

    public static DiskCachedWebImageLoader ImageLoaderInstance { get; } = new DiskCachedWebImageLoader();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {

        // If you use CommunityToolkit, line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        // Register all the services needed for the application to run
        var collection = new ServiceCollection();

        collection.AddSingleton<IMassWsClient, MassWsClient>();
        collection.AddSingleton<IScreen, MainWindowViewModel>();
        collection.AddSingleton<IPlayersService, PlayersService>();

        collection.AddTransient<AlbumsListViewModel>();
        collection.AddTransient<AlbumViewModel>();
        collection.AddTransient<LoginViewModel>();
        collection.AddTransient<HomeViewModel>();
        collection.AddTransient<PlaylistViewModel>();
        collection.AddTransient<ArtistViewModel>();

        collection.AddTransient<SearchViewModel>();
        collection.AddSingleton<SettingsViewModel>();
        collection.AddSingleton<PlayersViewModel>();
        collection.AddTransient<ArtistsViewModel>();
        collection.AddTransient<LibraryViewModel>();
        collection.AddTransient<RecommendationViewModel>();


        //settings
        var settings = new ConfigurationBuilder<ISettings>()
            .UseJsonFile("settings.json")
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
            singleViewPlatform.MainView = new MainWindow()
            {
                DataContext = vm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}