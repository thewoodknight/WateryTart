using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Windows.Input;
using WateryTart.Core.Services;

namespace WateryTart.Core.ViewModels.Players
{
    public partial class MiniPlayerViewModel : ReactiveObject, IViewModelBase
    {
        public string? UrlPathSegment { get; } = "MiniPlayerViewModel";
        public IScreen HostScreen { get; }
        [Reactive] public partial IColourService ColourService { get; set; }
        [Reactive] public partial string? Title { get; set; }
        public bool ShowMiniPlayer => false;
        public bool ShowNavigation => false;
        public bool ShowBackButton => true;
        [Reactive] public partial IPlayersService PlayersService { get; set; }
        public ICommand PlayerNextCommand { get; set; }
        public ICommand PlayerPlayPauseCommand { get; set; }
        public ICommand PlayPreviousCommand { get; set; }

        [ReactiveCommand]
        private void Clicked()
        {
            var vm = App.Container?.GetRequiredService<BigPlayerViewModel>();
            if (vm != null) 
                HostScreen.Router.Navigate.Execute(vm);
        }

        public MiniPlayerViewModel(IPlayersService playersService, IScreen screen, IColourService colourService)
        {
            PlayersService = playersService;
            HostScreen = screen;
            ColourService = colourService;

            PlayPreviousCommand = new RelayCommand(() => PlayersService.PlayerPrevious());
            PlayerNextCommand = new RelayCommand(() => PlayersService.PlayerNext());
            PlayerPlayPauseCommand = new RelayCommand(() => PlayersService.PlayerPlayPause());
        }
    }
}


