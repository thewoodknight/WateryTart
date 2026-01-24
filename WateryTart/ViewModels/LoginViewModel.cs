using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Windows.Input;
using WateryTart.MassClient;
using WateryTart.Settings;

namespace WateryTart.ViewModels
{
    public class LoginViewModel : ReactiveObject, IRoutableViewModel
    {
        private readonly IMassWSClient _massClient;
        private readonly ISettings _settings;
        private readonly IScreen _screen;
        public string? UrlPathSegment { get; }
        public IScreen HostScreen { get; }

        public string Server { get; set; }
        public string Username { get; set; }

        [Reactive] public string Password { get; set; }

        public ICommand LoginCommand { get; }

        public LoginViewModel(IScreen screen, IMassWSClient massClient, ISettings settings)
        {
            _massClient = massClient;
            _settings = settings;
            _screen = screen;


            LoginCommand = ReactiveCommand.Create(Login);
        }

        public async Task Login()
        {
            var x = await _massClient.Login(Username, Password, string.Format("ws://{0}/ws", Server));
            _settings.Credentials.BaseUrl = x.BaseUrl;
            _settings.Credentials.Token = x.Token;

            _screen.Router.NavigateBack.Execute();
        }
    }
}
