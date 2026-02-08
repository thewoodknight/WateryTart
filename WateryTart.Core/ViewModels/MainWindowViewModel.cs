using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WateryTart.Core.Messages;
using WateryTart.Core.Playback;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.ViewModels.Players;
using WateryTart.MusicAssistant;
using Xaml.Behaviors.SourceGenerators;

namespace WateryTart.Core.ViewModels;

public partial class MainWindowViewModel : ReactiveObject, IScreen, IActivatableViewModel
{
    private readonly IWsClient _massClient;
    private readonly SendSpinClient _sendSpinClient;
    private readonly ISettings _settings;
    private readonly ILogger<MainWindowViewModel> _logger;
    
    private bool _canNavigateToHome = true;
    private bool _canNavigateToMusic = true;
    private bool _canNavigateToSearch = true;
    private bool _canNavigateToSettings = true;
    private bool _canNavigateToPlayers = true;
    private bool _canNavigateBack = true;

    public ViewModelActivator? Activator { get; }
    public RelayCommand CloseSlideupCommand { get; }
    public IColourService ColourService { get; }
    [Reactive] public partial IViewModelBase CurrentViewModel { get; set; }
    public RelayCommand GoBack { get; }
    public RelayCommand GoHome { get; }
    public RelayCommand GoMusic { get; }
    public RelayCommand GoPlayers { get; }
    public RelayCommand GoSearch { get; }
    public RelayCommand GoSettings { get; }
    [Reactive] public partial bool IsMiniPlayerVisible { get; set; }
    [Reactive] public partial MiniPlayerViewModel MiniPlayer { get; set; }
    [Reactive] public partial IPlayersService PlayersService { get; set; }
    public RoutingState Router { get; } = new();
    public ISettings Settings => _settings;
    [Reactive] public partial bool ShowSlideupMenu { get; set; }
    [Reactive] public partial ReactiveObject? SlideupMenu { get; set; } = new();
    [Reactive] public partial string Title { get; set; }

    public MainWindowViewModel(IWsClient massClient, IPlayersService playersService, ISettings settings, IColourService colourService, SendSpinClient sendSpinClient, ILoggerFactory loggerFactory)
    {
        _massClient = massClient;
        PlayersService = playersService;
        _settings = settings;
        _sendSpinClient = sendSpinClient;
        ColourService = colourService;
        _logger = loggerFactory.CreateLogger<MainWindowViewModel>();
        ShowSlideupMenu = false;
        Activator = new ViewModelActivator();

        // Create the commands first
        GoBack = new RelayCommand(() => Router.NavigateBack.Execute(Unit.Default), () => _canNavigateBack);
        GoHome = new RelayCommand(() => Router.Navigate.Execute(App.Container.GetRequiredService<HomeViewModel>()), () => _canNavigateToHome);
        GoMusic = new RelayCommand(() => Router.Navigate.Execute(App.Container.GetRequiredService<LibraryViewModel>()), () => _canNavigateToMusic);
        GoSearch = new RelayCommand(() => Router.Navigate.Execute(App.Container.GetRequiredService<SearchViewModel>()), () => _canNavigateToSearch);
        GoSettings = new RelayCommand(() => Router.Navigate.Execute(App.Container.GetRequiredService<SettingsViewModel>()), () => _canNavigateToSettings);
        GoPlayers = new RelayCommand(() => Router.Navigate.Execute(App.Container.GetRequiredService<PlayersViewModel>()), () => _canNavigateToPlayers);
        CloseSlideupCommand = new RelayCommand(CloseMenu);

        // Subscribe to CurrentViewModel changes and update canExecute predicates
        Router.CurrentViewModel.Subscribe(vm =>
        {
            _canNavigateToHome = vm is not HomeViewModel;
            _canNavigateToMusic = vm is not LibraryViewModel;
            _canNavigateToSearch = vm is not SearchViewModel;
            _canNavigateToSettings = vm is not SettingsViewModel;
            _canNavigateToPlayers = vm is not PlayersViewModel;
            _canNavigateBack = Router.NavigationStack.Count > 1;

            // Notify commands to re-evaluate their canExecute predicates
            GoBack.NotifyCanExecuteChanged();
            GoHome.NotifyCanExecuteChanged();
            GoMusic.NotifyCanExecuteChanged();
            GoSearch.NotifyCanExecuteChanged();
            GoSettings.NotifyCanExecuteChanged();
            GoPlayers.NotifyCanExecuteChanged();
        });

        Router.CurrentViewModel.Subscribe(vm =>
        {
            if (vm is IViewModelBase ivmb)
            {
                ivmb.WhenAnyValue(x => x.Title)
                    .BindTo(this, v => v.Title);

                CurrentViewModel = ivmb;
                MiniPlayer = App.Container.GetRequiredService<MiniPlayerViewModel>();
            }
        });

        // Subscribe to changes in CurrentViewModel and SelectedPlayer to update IsMiniPlayerVisible
        this.WhenAnyValue(
            x => x.CurrentViewModel.ShowMiniPlayer,
            x => x.PlayersService.SelectedPlayer,
            (showMiniPlayer, selectedPlayer) => showMiniPlayer && selectedPlayer != null)
            .Subscribe(isVisible => IsMiniPlayerVisible = isVisible);

        MessageBus.Current.Listen<FromLoginMessage>().Subscribe(x => { _ = Connect(); });

        MessageBus.Current.Listen<MenuViewModel>()
            .Subscribe(
                x =>
                {
                    _logger.LogDebug("Menu view model received");
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        SlideupMenu = x;
                        ShowSlideupMenu = true;
                    });
                }
            );

        MessageBus.Current.Listen<CloseMenuMessage>()
            .Subscribe(
                x =>
                {
                    CloseMenu();
                });
    }

    [GenerateTypedAction]
    [GenerateTypedAction(UseDispatcher = true)]
    public void CloseMenuClicked()
    {
        CloseMenu();
    }

    private void CloseMenu()
    {
        _logger.LogDebug("Close menu clicked");
        ShowSlideupMenu = false;
        SlideupMenu = null;
    }

    public async Task Connect()
    {
        if (string.IsNullOrEmpty(_settings.Credentials?.Token))
        {
            _logger.LogInformation("No credentials found, navigating to login");
            var loginViewModel = App.Container?.GetRequiredService<LoginViewModel>();
            if (loginViewModel != null)
            {
                Router.Navigate.Execute(loginViewModel);
            }

            return;
        }

        _logger.LogInformation("Attempting to connect to MassClient");
        var connected = await _massClient.Connect(_settings.Credentials);

        if (!connected)
        {
            _logger.LogError("Failed to connect to MassClient");
            return;
        }

        _logger.LogInformation("Successfully connected to MassClient");
        await PlayersService.GetPlayers();

        Router.NavigateAndReset.Execute(App.Container.GetRequiredService<HomeViewModel>());
        
        _ = _sendSpinClient.ConnectAsync(_settings.Credentials.BaseUrl);
    }
}