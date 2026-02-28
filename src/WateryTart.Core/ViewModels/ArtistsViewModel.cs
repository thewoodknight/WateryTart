using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.ViewModels;

public partial class ArtistsViewModel : ViewModelBase<ArtistsViewModel>
{
    private const int PageSize = 50;
    [Reactive] public partial ObservableCollection<ArtistViewModel> Artists { get; set; } = [];
    public RelayCommand<ArtistViewModel> ClickedCommand { get; }
    [Reactive] public partial int CurrentOffset { get; set; } = 0;
    [Reactive] public partial bool HasMoreItems { get; set; } = true;
    public AsyncRelayCommand LoadMoreCommand { get; }

    public ArtistsViewModel(MusicAssistantClient massClient, IScreen screen, PlayersService playersService, ILoggerFactory loggerFactory)
        : base(loggerFactory, massClient, playersService, screen)
    {
        Title = "Artists";

        ClickedCommand = new RelayCommand<ArtistViewModel>(item =>
        {
            if (item != null)
                HostScreen.Router.Navigate.Execute(item);
        });

        LoadMoreCommand = new AsyncRelayCommand(
            LoadMoreAsync,
            () => !IsLoading && HasMoreItems
        );

        _ = LoadInitialAsync();
    }

    private async Task LoadArtistsAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;

            var response = await _client.WithWs().GetArtistsAsync(limit: PageSize, offset: CurrentOffset);

            if (response?.Result != null)
            {
                foreach (var artist in response.Result)
                {
                    Artists.Add(new ArtistViewModel(_client, HostScreen, _playersService!, artist));
                }

                HasMoreItems = response.Result.Count == PageSize;

                if (HasMoreItems)
                {
                    CurrentOffset += PageSize;
                }
            }
            else
            {
                HasMoreItems = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading artists: {ex.Message}");
            HasMoreItems = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadInitialAsync()
    {
        CurrentOffset = 0;
        Artists.Clear();
        await LoadArtistsAsync();
    }

    private async Task LoadMoreAsync()
    {
        await LoadArtistsAsync();
    }
}