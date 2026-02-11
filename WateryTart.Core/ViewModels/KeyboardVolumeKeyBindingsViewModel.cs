using ReactiveUI;
using Splat;
using System;
using System.Reactive;
using CommunityToolkit.Mvvm.Input;
using WateryTart.Core.ViewModels;
using Material.Icons;

namespace WateryTart.Core.ViewModels;

public class KeyboardVolumeKeyBindingsViewModel : ReactiveObject, IViewModelBase, IHaveSettings
{
    private string _playBinding = "P";
    private string _nextBinding = "N";
    private string _previousBinding = "B";
    private string _volumeUpBinding = "↑";
    private string _volumeDownBinding = "↓";
    private string _muteBinding = "M";
    private bool _isRecording;
    private string? _recordingFor;

    public string? UrlPathSegment => null;
    public IScreen HostScreen { get; }
    public string Title { get; set; } = "Keyboard Bindings";
    public bool ShowMiniPlayer => false;
    public bool ShowNavigation => false;

    public string PlayBinding
    {
        get => _playBinding;
        set => this.RaiseAndSetIfChanged(ref _playBinding, value);
    }

    public string NextBinding
    {
        get => _nextBinding;
        set => this.RaiseAndSetIfChanged(ref _nextBinding, value);
    }

    public string PreviousBinding
    {
        get => _previousBinding;
        set => this.RaiseAndSetIfChanged(ref _previousBinding, value);
    }

    public string VolumeUpBinding
    {
        get => _volumeUpBinding;
        set => this.RaiseAndSetIfChanged(ref _volumeUpBinding, value);
    }

    public string VolumeDownBinding
    {
        get => _volumeDownBinding;
        set => this.RaiseAndSetIfChanged(ref _volumeDownBinding, value);
    }

    public string MuteBinding
    {
        get => _muteBinding;
        set => this.RaiseAndSetIfChanged(ref _muteBinding, value);
    }

    public bool IsRecording
    {
        get => _isRecording;
        set => this.RaiseAndSetIfChanged(ref _isRecording, value);
    }

    public RelayCommand<string> RecordKeyCommand { get; }
    public RelayCommand ResetToDefaultsCommand { get; }

    public KeyboardVolumeKeyBindingsViewModel(IScreen? hostScreen = null)
    {
        HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>() ?? throw new InvalidOperationException("IScreen not registered");

        RecordKeyCommand = new RelayCommand<string>((s) => ExecuteRecordKey(s));
        ResetToDefaultsCommand = new RelayCommand(ExecuteResetToDefaults);
    }

    private void ExecuteRecordKey(string bindingName)
    {
        _recordingFor = bindingName;
        IsRecording = true;

        // TODO: Hook into global keyboard listener
        // Listen for next key press and update the appropriate binding
        // Then set IsRecording = false

        // Placeholder: simulate key recording after 2 seconds
        // In production, use native keyboard hooks or Avalonia's KeyDown event
    }

    private void ExecuteResetToDefaults()
    {
        PlayBinding = "P";
        NextBinding = "N";
        PreviousBinding = "B";
        VolumeUpBinding = "↑";
        VolumeDownBinding = "↓";
        MuteBinding = "M";
    }

    /// <summary>
    /// Call this from your global keyboard handler when a key is pressed during recording.
    /// </summary>
    public void SetRecordedKey(string keyName)
    {
        if (!IsRecording || _recordingFor == null)
            return;

        switch (_recordingFor)
        {
            case "Play":
                PlayBinding = keyName;
                break;
            case "Next":
                NextBinding = keyName;
                break;
            case "Previous":
                PreviousBinding = keyName;
                break;
            case "VolumeUp":
                VolumeUpBinding = keyName;
                break;
            case "VolumeDown":
                VolumeDownBinding = keyName;
                break;
            case "Mute":
                MuteBinding = keyName;
                break;
        }

        IsRecording = false;
        _recordingFor = null;
    }

    public MaterialIconKind Icon => MaterialIconKind.Keyboard;
}