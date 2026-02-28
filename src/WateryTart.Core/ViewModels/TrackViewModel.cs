using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WateryTart.Core.Services;
using WateryTart.Core.ViewModels.Menus;
using WateryTart.Core.ViewModels.Popups;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.ViewModels;

public partial class TrackViewModel : ViewModelBase<TrackViewModel>, IDisposable
{
    private readonly CompositeDisposable _disposables = [];
    private bool _isNowPlaying;
    private Item? _track = null;

    public bool IsNowPlaying
    {
        get => _isNowPlaying;
        private set => this.RaiseAndSetIfChanged(ref _isNowPlaying, value);
    }

    public Item? Track
    {
        get => _track;
        set => this.RaiseAndSetIfChanged(ref _track, value);
    }

    public RelayCommand TrackAltMenuCommand { get; }
    public RelayCommand TrackFullViewCommand { get; }

    public TrackViewModel(MusicAssistantClient massClient, PlayersService playersService, Item? t = null)
        : base(client: massClient, playersService: playersService)  
    {
        Track = t;

        // Monitor for changes to the currently playing track
        _disposables.Add(
            this.WhenAnyValue(
                x => x._playersService!.SelectedPlayer,
                x => x._playersService!.SelectedPlayer!.CurrentMedia,
                x => x._playersService!.SelectedPlayer!.CurrentMedia!.Uri,
                x => x.Track.Uri)
                .Select(_ => _playersService!.SelectedPlayer?.CurrentMedia?.Uri == Track?.Uri)
                .DistinctUntilChanged()
                .Subscribe(isPlaying => IsNowPlaying = isPlaying)
        );

        TrackFullViewCommand = new RelayCommand(() =>
        {
            if (Track != null)
                MessageBus.Current.SendMessage<IPopupViewModel>(MenuHelper.BuildStandardPopup(_playersService!, Track));
        });

        TrackAltMenuCommand = new RelayCommand(() =>
        {
            if (Track != null)
                MessageBus.Current.SendMessage< IPopupViewModel>(MenuHelper.BuildStandardPopup(_playersService!, Track));
        });
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }

    public async Task LoadFromId(string itemId, string provider)
    {
        try
         {
             var response = await _client.WithWs().GetLibraryItemAsync(MediaType.Track, itemId, provider);
             if (response?.Result != null)
             {
                 Track = response.Result;
                 Title = Track.Name;
             }
         }
         catch (Exception ex)
         {
             System.Diagnostics.Debug.WriteLine($"Error loading track: {ex.Message}");
         }
    }
}