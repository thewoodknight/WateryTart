using ReactiveUI.Avalonia;
using WateryTart.Platform.Windows.ViewModels;
using System;
using Avalonia.Controls;
using WateryTart.Core.Settings;

namespace WateryTart.Platform.Windows.Views;

public partial class PlaybackSettingsView : ReactiveUserControl<PlaybackSettingsViewModel>
{
    public PlaybackSettingsView()
    {
        InitializeComponent();

        // Populate ComboBox items with enum values
        try
        {
            var combo = this.FindControl<ComboBox>("BackendComboBox");
            combo.ItemsSource = Enum.GetValues(typeof(PlaybackBackend));

            // Keep selection in sync with viewmodel
            this.DataContextChanged += (s, e) =>
            {
                try
                {
                    if (this.DataContext is PlaybackSettingsViewModel vm)
                        combo.SelectedItem = vm.SelectedBackend;
                }
                catch { }
            };

            combo.SelectionChanged += (s, e) =>
            {
                try
                {
                    if (this.DataContext is PlaybackSettingsViewModel vm && combo.SelectedItem is PlaybackBackend pb)
                        vm.SelectedBackend = pb;
                }
                catch { }
            };
        }
        catch { }
    }
}
