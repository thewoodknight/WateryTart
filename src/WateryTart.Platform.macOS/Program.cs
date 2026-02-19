using Avalonia;
using ReactiveUI.Avalonia;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Velopack;
using WateryTart.Core;
using WateryTart.Core.Playback;

namespace WateryTart.Platform.macOS;

sealed class Program
{
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
        VelopackApp.Build().Run();

        // Set the process name for macOS menu bar display
        SetMacOSProcessName("WateryTart");
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>(() =>
        {
            var x = new App(
                [
                    new InstancePlatformSpecificRegistration<IPlayerFactory>(new MacOSAudioPlayerFactory()),
                    // No GPIO on macOS - skip volume encoder registration
                ]);

            return x;
        })
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .WithDeveloperTools()
            .LogToTrace();
}
