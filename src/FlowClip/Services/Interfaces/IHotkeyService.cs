using System.Windows;

namespace FlowClip.Services.Interfaces;

/// <summary>
/// Service for managing global hotkeys.
/// </summary>
public interface IHotkeyService : IDisposable
{
    /// <summary>
    /// Fired when the registered hotkey is pressed.
    /// </summary>
    event EventHandler? HotkeyPressed;

    /// <summary>
    /// Register the global hotkey.
    /// </summary>
    /// <param name="window">The window to receive hotkey messages.</param>
    /// <param name="modifiers">Modifier keys (Ctrl=1, Alt=2, Shift=4, Win=8).</param>
    /// <param name="key">Virtual key code.</param>
    bool Register(Window window, int modifiers, int key);

    /// <summary>
    /// Unregister the global hotkey.
    /// </summary>
    void Unregister();

    /// <summary>
    /// Update the hotkey registration with new keys.
    /// </summary>
    bool UpdateHotkey(int modifiers, int key);
}
