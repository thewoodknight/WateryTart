using SharpHook;
using SharpHook.Providers;

namespace WateryTart.Core.Services;

public class WindowsVolumeService : IVolumeService, IReaper
{
    private readonly PlayersService playerService;
    private EventLoopGlobalHook _hook;

    public bool IsEnabled { get; set; }

    public WindowsVolumeService(PlayersService playerService)
    {
        UioHookProvider.Instance.KeyTypedEnabled = false;
        _hook = new EventLoopGlobalHook(SharpHook.Data.GlobalHookType.Keyboard);
        _hook.HookEnabled += OnHookEnabled;
        _hook.HookDisabled += OnHookDisabled;
        _hook.KeyReleased += OnKeyReleased;

        _hook.RunAsync();
        this.playerService = playerService;
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
                playerService.PlayerVolumeUp();
                break;
            case SharpHook.Data.KeyCode.VcVolumeDown:
                playerService.PlayerVolumeDown();
                break;
            case SharpHook.Data.KeyCode.VcMediaPlay:
                playerService.PlayerPlayPause();
                break;
            case SharpHook.Data.KeyCode.VcMediaNext:
                playerService.PlayerNext();
                break;
            case SharpHook.Data.KeyCode.VcMediaPrevious:
                playerService.PlayerPrevious();
                break;
        }
    }

    public void Reap()
    {
        _hook?.Stop();
        _hook?.Dispose();
    }
}
