using FlowClip.Models;

namespace FlowClip.Services.Interfaces;

/// <summary>
/// Service for managing application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Get the current settings.
    /// </summary>
    Task<AppSettings> GetSettingsAsync();

    /// <summary>
    /// Save settings to database.
    /// </summary>
    Task SaveSettingsAsync(AppSettings settings);

    /// <summary>
    /// Update widget position.
    /// </summary>
    Task SaveWidgetPositionAsync(double x, double y);

    /// <summary>
    /// Update panel expanded state.
    /// </summary>
    Task SavePanelStateAsync(bool isExpanded);

    /// <summary>
    /// Enable or disable Windows startup.
    /// </summary>
    void SetStartupEnabled(bool enabled);

    /// <summary>
    /// Check if startup is currently enabled.
    /// </summary>
    bool IsStartupEnabled { get; }
}
