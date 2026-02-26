using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using WateryTart.Core.ViewModels;
using WateryTart.Core.ViewModels.Players;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.Views;
using WateryTart.Core.Views.Players;
using WateryTart.Core.Views.Menus;

namespace WateryTart.Core;

/// <summary>
/// NativeAOT-compatible ViewLocator using static view registration.
/// No reflection, assembly scanning, or Activator.CreateInstance calls.
/// </summary>
public class ViewLocator : IDataTemplate
{
    // Static dictionary mapping ViewModel types to View factory functions
    public static readonly Dictionary<Type, Func<Control>> _viewFactories = new()
    {
        // Main navigation views
        [typeof(HomeViewModel)] = () => new HomeView(),
        [typeof(Home2ViewModel)] = () => new Home2View(),
        [typeof(LibraryViewModel)] = () => new LibraryView(),
        [typeof(SearchViewModel)] = () => new SearchView(),
        [typeof(SettingsViewModel)] = () => new SettingsView(),
        [typeof(PlayersViewModel)] = () => new PlayersView(),
        [typeof(LoginViewModel)] = () => new LoginView(),
        
        // Content views
        [typeof(AlbumViewModel)] = () => new AlbumView(),
        [typeof(AlbumsListViewModel)] = () => new AlbumsListView(),
        [typeof(ArtistViewModel)] = () => new ArtistView(),
        [typeof(ArtistsViewModel)] = () => new ArtistsView(),
        [typeof(PlaylistViewModel)] = () => new PlaylistView(),
        [typeof(PlaylistsViewModel)] = () => new PlaylistsView(),
        [typeof(TracksViewModel)] = () => new TracksView(),
        [typeof(RecommendationViewModel)] = () => new RecommendationView(),
        [typeof(SearchResultsViewModel)] = () => new SearchResultsView(),
        [typeof(SimilarTracksViewModel)] = () => new SimilarTracksView(),

        // Player views
        [typeof(MiniPlayerViewModel)] = () => new MiniPlayerView(),
        [typeof(BigPlayerViewModel)] = () => new BigPlayerView(),
        
        [typeof(MenuViewModel)] = () => new MenuView(),
        
        // Settings views
        [typeof(ServerSettingsViewModel)] = () => new ServerSettingsView(),
        [typeof(KeyboardVolumeKeyBindingsViewModel)] = () => new KeyboardVolumeKeyBindingsView(),
        [typeof(LoggerSettingsViewModel)] = () => new LoggerSettingsView(),
        [typeof(GeneralSettingsViewModel)] = () => new GeneralSettingsView(),
    };

    public Control Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock { Text = "Data was null" };
        }

        var viewModelType = data.GetType();
        
        if (_viewFactories.TryGetValue(viewModelType, out var factory))
        {
            return factory();
        }

        // Fallback for unmapped ViewModels
        return new TextBlock 
        { 
            Text = $"No view registered for: {viewModelType.Name}"
        };
    }

    public bool Match(object? data)
    {
        return data is IViewModelBase;
    }

    /// <summary>
    /// Register a custom ViewModel-to-View mapping at runtime.
    /// Useful for platform-specific views or plugin scenarios.
    /// </summary>
    public static void RegisterView<TViewModel, TView>() 
        where TViewModel : class
        where TView : Control, new()
    {
        _viewFactories[typeof(TViewModel)] = () => new TView();
    }

    /// <summary>
    /// Register a custom ViewModel-to-View mapping with a factory function.
    /// </summary>
    public static void RegisterView<TViewModel>(Func<Control> factory) 
        where TViewModel : class
    {
        _viewFactories[typeof(TViewModel)] = factory;
    }
}