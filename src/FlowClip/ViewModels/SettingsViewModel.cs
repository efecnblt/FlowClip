using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowClip.Models;
using FlowClip.Services.Interfaces;

namespace FlowClip.ViewModels;

/// <summary>
/// ViewModel for the Settings window.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IHotkeyService _hotkeyService;
    private AppSettings _settings = new();

    [ObservableProperty]
    private int _historyLimit = 50;

    [ObservableProperty]
    private bool _runOnStartup;

    [ObservableProperty]
    private bool _useCtrl = true;

    [ObservableProperty]
    private bool _useAlt;

    [ObservableProperty]
    private bool _useShift = true;

    [ObservableProperty]
    private string _hotkeyKey = "V";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel(ISettingsService settingsService, IHotkeyService hotkeyService)
    {
        _settingsService = settingsService;
        _hotkeyService = hotkeyService;
    }

    /// <summary>
    /// Load settings from database.
    /// </summary>
    public async Task LoadAsync()
    {
        _settings = await _settingsService.GetSettingsAsync();

        HistoryLimit = _settings.HistoryLimit;
        RunOnStartup = _settings.RunOnStartup;

        // Parse modifiers
        UseCtrl = (_settings.HotkeyModifiers & 2) != 0;  // MOD_CONTROL
        UseAlt = (_settings.HotkeyModifiers & 1) != 0;   // MOD_ALT
        UseShift = (_settings.HotkeyModifiers & 4) != 0; // MOD_SHIFT

        // Parse key
        HotkeyKey = ((Key)_settings.HotkeyKey).ToString();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Build modifiers
        int modifiers = 0;
        if (UseCtrl) modifiers |= 2;   // MOD_CONTROL
        if (UseAlt) modifiers |= 1;    // MOD_ALT
        if (UseShift) modifiers |= 4;  // MOD_SHIFT

        // Parse key
        if (!Enum.TryParse<Key>(HotkeyKey, true, out var key))
        {
            StatusMessage = "Invalid hotkey key";
            return;
        }

        _settings.HistoryLimit = HistoryLimit;
        _settings.RunOnStartup = RunOnStartup;
        _settings.HotkeyModifiers = modifiers;
        _settings.HotkeyKey = KeyInterop.VirtualKeyFromKey(key);

        await _settingsService.SaveSettingsAsync(_settings);

        // Update hotkey registration
        var success = _hotkeyService.UpdateHotkey(modifiers, _settings.HotkeyKey);
        if (!success)
        {
            StatusMessage = "Failed to register hotkey. It may be in use by another application.";
            return;
        }

        StatusMessage = "Settings saved!";
    }

    [RelayCommand]
    private void Cancel()
    {
        // Close window (handled by view)
    }

    /// <summary>
    /// Get the current hotkey display string.
    /// </summary>
    public string HotkeyDisplay
    {
        get
        {
            var parts = new List<string>();
            if (UseCtrl) parts.Add("Ctrl");
            if (UseAlt) parts.Add("Alt");
            if (UseShift) parts.Add("Shift");
            parts.Add(HotkeyKey);
            return string.Join(" + ", parts);
        }
    }
}
