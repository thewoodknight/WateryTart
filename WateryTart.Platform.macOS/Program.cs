using Avalonia;
using ReactiveUI.Avalonia;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
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

    // ObjC runtime imports for setting process name on macOS
    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string className);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string selectorName);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_string(IntPtr receiver, IntPtr selector, string arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_setName(IntPtr receiver, IntPtr selector, IntPtr arg);

    private static void SetMacOSProcessName(string name)
    {
        try
        {
            // Get NSProcessInfo class
            var processInfoClass = objc_getClass("NSProcessInfo");
            var processInfoSel = sel_registerName("processInfo");
            var processInfo = objc_msgSend(processInfoClass, processInfoSel);

            // Create NSString for the new name
            var nsStringClass = objc_getClass("NSString");
            var stringWithUtf8Sel = sel_registerName("stringWithUTF8String:");
            var nsName = objc_msgSend_string(nsStringClass, stringWithUtf8Sel, name);

            // Set process name
            var setProcessNameSel = sel_registerName("setProcessName:");
            objc_msgSend_setName(processInfo, setProcessNameSel, nsName);

            Console.WriteLine($"Set macOS process name to: {name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to set macOS process name: {ex.Message}");
        }
    }

    [STAThread]
    public static void Main(string[] args)
    {
        // Set the process name for macOS menu bar display
        SetMacOSProcessName("WateryTart");

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
