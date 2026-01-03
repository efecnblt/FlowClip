namespace FlowClip.Services.Interfaces;

/// <summary>
/// Service for managing the system tray icon.
/// </summary>
public interface ITrayIconService : IDisposable
{
    /// <summary>
    /// Fired when "Show" is clicked in tray menu.
    /// </summary>
    event EventHandler? ShowRequested;

    /// <summary>
    /// Fired when "Settings" is clicked in tray menu.
    /// </summary>
    event EventHandler? SettingsRequested;

    /// <summary>
    /// Fired when "Exit" is clicked in tray menu.
    /// </summary>
    event EventHandler? ExitRequested;

    /// <summary>
    /// Initialize and show the tray icon.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Show a balloon notification.
    /// </summary>
    void ShowNotification(string title, string message);

    /// <summary>
    /// Update tray icon tooltip.
    /// </summary>
    void SetTooltip(string tooltip);
}
