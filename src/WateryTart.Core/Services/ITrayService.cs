using Avalonia.Controls;

namespace WateryTart.Core.Services;

public interface ITrayService
{
    void CreateTrayIcon();
    void Initialize(Window mainWindow);
    void Dispose();
}
