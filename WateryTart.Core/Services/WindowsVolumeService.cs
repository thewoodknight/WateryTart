using SharpHook;
using SharpHook.Providers;

namespace WateryTart.Core.Services;

public interface IReaper
{
    void Reap();
}
public class WindowsVolumeService : IVolumeService, IReaper
{
    private readonly IPlayersService playerService;
    private EventLoopGlobalHook _hook;

    public bool IsEnabled { get; set; }

    public WindowsVolumeService(IPlayersService playerService)
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
        }
    }

    public void Reap()
    {
        _hook?.Stop();
        _hook?.Dispose();
    }
}
