using System.ComponentModel.DataAnnotations;

namespace FlowClip.Models;

/// <summary>
/// Application settings stored in the database.
/// </summary>
public class AppSettings
{
    [Key]
    public int Id { get; set; } = 1;

    /// <summary>
    /// Maximum number of entries to keep in history.
    /// </summary>
    public int HistoryLimit { get; set; } = 50;

    /// <summary>
    /// Global hotkey modifier keys (Ctrl=1, Alt=2, Shift=4, Win=8).
    /// </summary>
    public int HotkeyModifiers { get; set; } = 5; // Ctrl + Shift

    /// <summary>
    /// Global hotkey virtual key code.
    /// </summary>
    public int HotkeyKey { get; set; } = 0x56; // V key

    /// <summary>
    /// Whether to run at Windows startup.
    /// </summary>
    public bool RunOnStartup { get; set; }

    /// <summary>
    /// Current theme (Dark/Light).
    /// </summary>
    public string Theme { get; set; } = "Dark";

    /// <summary>
    /// Saved X position of the widget.
    /// </summary>
    public double? WidgetPositionX { get; set; }

    /// <summary>
    /// Saved Y position of the widget.
    /// </summary>
    public double? WidgetPositionY { get; set; }

    /// <summary>
    /// Whether the history panel is currently expanded.
    /// </summary>
    public bool IsPanelExpanded { get; set; }
}
