using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive;
using System.Runtime;
using System.Threading.Tasks;
using WateryTart.Core.Settings;
using WateryTart.Service.MassClient;
using WateryTart.Service.MassClient.Models.Auth;

namespace WateryTart.Core.ViewModels;

public class FromLoginMessage()
{

}
public partial class LoginViewModel : ReactiveObject, IViewModelBase
{
    private readonly IMassWsClient _massClient;
    private readonly ISettings _settings;
    private readonly IScreen _screen;

    public string? UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; set; }
    public bool ShowMiniPlayer { get; }
    public bool ShowNavigation { get; }

    [Reactive] public partial string Server { get; set; }

    [Reactive] public partial string Username { get; set; }

    [Reactive] public partial string Password { get; set; }

    [Reactive] public partial string ErrorMessage { get; set; }

    [Reactive] public partial bool HasError { get; set; }

    [Reactive] public partial bool IsLoading { get; set; }
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }

    public LoginViewModel(IScreen screen, IMassWsClient massClient, ISettings settings)
    {
        _massClient = massClient;
        _settings = settings;
        _screen = screen;
        LoginCommand = ReactiveCommand.CreateFromTask(ExecuteLogin);
        //screen.Router.NavigateBack.Execute();
    }

    private async Task Login()
    {
        var x = await _massClient.Login(Username, Password, Server);

        if (x.Success)
        {
            _settings.Credentials = new MassCredentials()
            {

                BaseUrl = x.Credentials.BaseUrl,
                Token = x.Credentials.Token
            };

            MessageBus.Current.SendMessage(new FromLoginMessage());
            _screen.Router.NavigateBack.Execute();
            return;
        }
        SetError(x.Error);
    }

    private async Task ExecuteLogin()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // Validation
            if (string.IsNullOrWhiteSpace(Server))
            {
                SetError("Server address is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                SetError("Username is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                SetError("Password is required.");
                return;
            }

            Login();
        }
        catch (Exception ex)
        {
            SetError($"Login failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

}