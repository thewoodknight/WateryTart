using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace WateryTart.Platform.Windows;

internal static class NativeAotPreservation
{
    [ModuleInitializer]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Button))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TextBox))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ListBox))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ComboBox))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Control))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ContentControl))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(UserControl))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TemplatedControl))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(EventTriggerBehavior))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(InvokeCommandAction))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Behavior))]
    public static void Initialize()
    {
        // Preserves types for Native AOT - called automatically before Main()
    }
}