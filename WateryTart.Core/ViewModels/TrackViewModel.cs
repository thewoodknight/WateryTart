using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using WateryTart.Core.Services;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.ViewModels;

public class TrackViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    private readonly IScreen _screen;
    private readonly IPlayersService _playersService;

    private Item _track;
    public Item Track
    {
        get => _track;
        set => this.RaiseAndSetIfChanged(ref _track, value);
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

        TrackFullViewCommand = ReactiveCommand.Create(() =>
        {
            MessageBus.Current.SendMessage(MenuHelper.BuildStandardPopup(_playersService, Track));
        });

        TrackAltMenuCommand = ReactiveCommand.Create(() =>
        {
            MessageBus.Current.SendMessage(MenuHelper.BuildStandardPopup(_playersService, Track));
        });

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