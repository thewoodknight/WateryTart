using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using WateryTart.Core.Services;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public class TrackViewModel : ReactiveObject, IViewModelBase, IDisposable
{
    private readonly IMassWsClient _massClient;
    private readonly IScreen _screen;
    private readonly IPlayersService _playersService;
    private CompositeDisposable _disposables = new CompositeDisposable();

    private Item _track;
    public Item Track
    {
        get => _track;
        set => this.RaiseAndSetIfChanged(ref _track, value);
    }

    private bool _isNowPlaying;
    public bool IsNowPlaying
    {
        get => _isNowPlaying;
        private set => this.RaiseAndSetIfChanged(ref _isNowPlaying, value);
    }

    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; set; }
    public bool ShowMiniPlayer => true;
    public bool ShowNavigation => true;

    public ReactiveCommand<Unit, Unit> TrackAltMenuCommand { get; }
    public ReactiveCommand<Unit, Unit> TrackFullViewCommand { get; }

    public TrackViewModel(IMassWsClient massClient, IScreen screen, IPlayersService playersService, Item t = null)
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
                x => x._playersService.SelectedPlayer.CurrentMedia.uri,
                x => x.Track.Uri)
                .Select(_ => _playersService.SelectedPlayer?.CurrentMedia?.uri == Track?.Uri)
                .DistinctUntilChanged()
                .Subscribe(isPlaying => IsNowPlaying = isPlaying)
        );

        _disposables.Add(TrackFullViewCommand = ReactiveCommand.Create(() =>
        {
            MessageBus.Current.SendMessage(MenuHelper.BuildStandardPopup(_playersService, Track));
        }));

        _disposables.Add(TrackAltMenuCommand = ReactiveCommand.Create(() =>
        {
            MessageBus.Current.SendMessage(MenuHelper.BuildStandardPopup(_playersService, Track));
        }));
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

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}