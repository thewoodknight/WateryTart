using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;
using WateryTart.Services;
using WateryTart.Settings;

namespace WateryTart.ViewModels.Players
{
    public partial class MiniPlayerViewModel : ReactiveObject, IViewModelBase
    {
        public string? UrlPathSegment { get; } = "MiniPlayerViewModel";
        public IScreen HostScreen { get; }
        [Reactive] public partial string Title { get; set; }
        public ReactiveCommand<Unit, Unit> PlayNextCommand { get; }
        public ReactiveCommand<Unit, Unit> PlayerPlayPauseCommand { get; }
        public ReactiveCommand<Unit, Unit> PlayerPreviousCommand { get; }
        public bool ShowMiniPlayer => false;
        public bool ShowNavigation => false;
        public ReactiveCommand<Unit, Unit> ClickedCommand { get; }

        [Reactive] public partial IPlayersService PlayersService { get; set; }
        public IScreen Screen { get; }

        public MiniPlayerViewModel(IPlayersService playersService, IScreen screen)
        {
            PlayersService = playersService;
            Screen = screen;
            PlayNextCommand = ReactiveCommand.Create<Unit>(_ => PlayersService.PlayerNext());
            PlayerPlayPauseCommand = ReactiveCommand.Create<Unit>(_ => PlayersService.PlayerPlayPause());
            PlayerPreviousCommand = ReactiveCommand.Create<Unit>(_ => PlayersService.PlayerPrevious());

            ClickedCommand = ReactiveCommand.Create<Unit>(_ =>
            {
                var vm = WateryTart.App.Container.GetRequiredService<BigPlayerViewModel>();
                Screen.Router.Navigate.Execute(vm);
            }
            );
            DispatcherTimer t = new DispatcherTimer();
            t.Interval = new System.TimeSpan(0, 0, 1);
            t.Tick += T_Tick;
            t.Start();
        }

        private void T_Tick(object? sender, System.EventArgs e)
        {
            if (PlayersService.SelectedPlayer?.PlaybackState == MassClient.Models.PlaybackState.playing)
                PlayersService.SelectedPlayer?.CurrentMedia.elapsed_time += 1;
        }
    }
}


