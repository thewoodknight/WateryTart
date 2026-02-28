using SharpHook;
using SharpHook.Providers;

namespace WateryTart.Core.Services;

public class WindowsVolumeService : IVolumeService, IReaper
{
    private readonly EventLoopGlobalHook _hook;
    private readonly PlayersService _playerService;
    public bool IsEnabled { get; set; }

    public WindowsVolumeService(PlayersService playerService)
    {
        UioHookProvider.Instance.KeyTypedEnabled = false;
        _hook = new EventLoopGlobalHook(SharpHook.Data.GlobalHookType.Keyboard);
        _hook.HookEnabled += OnHookEnabled;
        _hook.HookDisabled += OnHookDisabled;
        _hook.KeyReleased += OnKeyReleased;

        _hook.RunAsync();
        _playerService = playerService;
    }

    public void Reap()
    {
        _hook?.Stop();
        _hook?.Dispose();
    }

    private void OnHookDisabled(object? sender, HookEventArgs e)
    {
    }

    private void OnHookEnabled(object? sender, HookEventArgs e)
    {
    }

    private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        switch (e.Data.KeyCode)
        {
            case SharpHook.Data.KeyCode.VcVolumeUp:
                _playerService?.PlayerVolumeUp();
                break;

            case SharpHook.Data.KeyCode.VcVolumeDown:
                _playerService?.PlayerVolumeDown();
                break;

            case SharpHook.Data.KeyCode.VcMediaPlay:
                _playerService?.PlayerPlayPause();
                break;

            case SharpHook.Data.KeyCode.VcMediaNext:
                _playerService?.PlayerNext();
                break;

            case SharpHook.Data.KeyCode.VcMediaPrevious:
                _playerService?.PlayerPrevious();
                break;
        }
    }
}