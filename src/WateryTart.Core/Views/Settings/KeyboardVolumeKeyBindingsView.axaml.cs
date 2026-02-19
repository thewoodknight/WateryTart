using Avalonia.Input;
using ReactiveUI.Avalonia;
using WateryTart.Core.ViewModels;

namespace WateryTart.Core.Views;

public partial class KeyboardVolumeKeyBindingsView : ReactiveUserControl<KeyboardVolumeKeyBindingsViewModel>
{
    public KeyboardVolumeKeyBindingsView()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is KeyboardVolumeKeyBindingsViewModel viewModel && viewModel.IsRecording)
        {
            // Get the key name with modifiers
            var keyName = GetKeyDisplayName(e);
            viewModel.SetRecordedKey(keyName);
            e.Handled = true;
        }
    }

    private static string GetKeyDisplayName(KeyEventArgs e)
    {
        var modifiers = e.KeyModifiers;
        var key = e.Key;

        var keyName = key.ToString();

        if (modifiers.HasFlag(KeyModifiers.Control))
            keyName = "Ctrl+" + keyName;
        if (modifiers.HasFlag(KeyModifiers.Alt))
            keyName = "Alt+" + keyName;
        if (modifiers.HasFlag(KeyModifiers.Shift))
            keyName = "Shift+" + keyName;

        return keyName;
    }
}