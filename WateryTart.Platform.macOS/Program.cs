using Avalonia;
using ReactiveUI.Avalonia;
using System;
using System.IO;
using System.Reflection;
using WateryTart.Core;
using WateryTart.Core.Playback;

namespace WateryTart.Platform.macOS;

sealed class Program
{
    private static readonly string LockFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WateryTart",
        "watertart.lock");

    private static FileStream? _lockFile;

    [STAThread]
    public static void Main(string[] args)
    {
        // Ensure directory exists
        var lockDir = Path.GetDirectoryName(LockFilePath)!;
        Directory.CreateDirectory(lockDir);

        // Try to acquire exclusive lock on the lock file
        try
        {
            _lockFile = new FileStream(
                LockFilePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);

            // Write PID to lock file for debugging
            using var writer = new StreamWriter(_lockFile, leaveOpen: true);
            writer.Write(Environment.ProcessId);
            writer.Flush();
        }
        catch (IOException)
        {
            // Another instance has the lock
            Console.WriteLine("WateryTart is already running. Only one instance is allowed.");
            Environment.Exit(1);
            return;
        }

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            _lockFile?.Dispose();
            try { File.Delete(LockFilePath); } catch { }
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>(() =>
        {
            var x = new App(
                [
                    new InstancePlatformSpecificRegistration(new MacOSAudioPlayerFactory()),
                    // No GPIO on macOS - skip volume encoder registration
                ], Assembly.GetExecutingAssembly());

            return x;
        })
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .WithDeveloperTools()
            .LogToTrace();
}
