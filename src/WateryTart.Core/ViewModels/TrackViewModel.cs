using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.ViewModels.Popups;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.ViewModels;

public partial class TrackViewModel : ReactiveObject, IViewModelBase, IDisposable
{
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private readonly MusicAssistantClient _massClient;
    private readonly IPlayersService _playersService;
    private readonly IScreen _screen;
    private bool _isNowPlaying;
    private Item? _track = null;
    public IScreen HostScreen { get; }
    [Reactive] public partial bool IsLoading { get; set; }

    public bool IsNowPlaying
    {
        get => _isNowPlaying;
        private set => this.RaiseAndSetIfChanged(ref _isNowPlaying, value);
    }

    public bool ShowMiniPlayer => true;

    public bool ShowNavigation => true;

    public string Title { get; set; } = string.Empty;

    public Item? Track
    {
        get => _track;
        set => this.RaiseAndSetIfChanged(ref _track, value);
    }

    public RelayCommand TrackAltMenuCommand { get; }
    public RelayCommand TrackFullViewCommand { get; }
    public string? UrlPathSegment { get; } = "Track/ID";

    public TrackViewModel(MusicAssistantClient massClient, IScreen screen, IPlayersService playersService, Item? t = null)
    {
        _massClient = massClient;
        _screen = screen;
        _playersService = playersService;
        HostScreen = screen;

        Track = t;

        // Monitor for changes to the currently playing track
        _disposables.Add(
            this.WhenAnyValue(
                x => x._playersService.SelectedPlayer,
                x => x._playersService.SelectedPlayer.CurrentMedia,
                x => x._playersService.SelectedPlayer.CurrentMedia.Uri,
                x => x.Track.Uri)
                .Select(_ => _playersService.SelectedPlayer?.CurrentMedia?.Uri == Track?.Uri)
                .DistinctUntilChanged()
                .Subscribe(isPlaying => IsNowPlaying = isPlaying)
        );

        TrackFullViewCommand = new RelayCommand(() =>
        {
            if (Track != null)
                MessageBus.Current.SendMessage<IPopupViewModel>(MenuHelper.BuildStandardPopup(_playersService, Track));
        });

        TrackAltMenuCommand = new RelayCommand(() =>
        {
            if (Track != null)
                MessageBus.Current.SendMessage< IPopupViewModel>(MenuHelper.BuildStandardPopup(_playersService, Track));
        });
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }

    public async Task LoadFromId(string itemId, string provider)
    {
        /* try
         {
             var response = await _massClient.GetTrackAsync(itemId, provider);
             if (response?.Result != null)
             {
                 Track = response.Result;
                 Title = Track.Name;
             }
         }
         catch (Exception ex)
         {
             System.Diagnostics.Debug.WriteLine($"Error loading track: {ex.Message}");
         }*/
    }
}