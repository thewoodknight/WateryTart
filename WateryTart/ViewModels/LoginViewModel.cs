using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Input;
using WateryTart.MassClient;
using WateryTart.Settings;

namespace WateryTart.ViewModels
{
    public partial class LoginViewModel : ReactiveObject, IViewModelBase
    {
        private readonly IMassWsClient _massClient;
        private readonly ISettings _settings;
        private readonly IScreen _screen;
        public string? UrlPathSegment { get; }
        public IScreen HostScreen { get; }

        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public ICommand LoginCommand { get; }
        public bool ShowMiniPlayer => false;
        public bool ShowNavigation => false;
        public LoginViewModel(IScreen screen, IMassWsClient massClient, ISettings settings)
        {
            _massClient = massClient;
            _settings = settings;
            _screen = screen;

            LoginCommand = ReactiveCommand.Create(Login);
        }

        public async Task Login()
        {
            var x = await _massClient.Login(Username, Password, Server);
            _settings.Credentials.BaseUrl = x.BaseUrl;
            _settings.Credentials.Token = x.Token;

            _screen.Router.NavigateBack.Execute();
        }

        public string Title { get; set; }
    }
}