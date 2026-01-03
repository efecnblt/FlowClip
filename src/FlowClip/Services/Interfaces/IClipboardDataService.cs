using FlowClip.Models;

namespace FlowClip.Services.Interfaces;

/// <summary>
/// Service for managing clipboard history data persistence.
/// </summary>
public interface IClipboardDataService
{
    /// <summary>
    /// Add a new clipboard entry.
    /// </summary>
    Task<ClipboardEntry> AddEntryAsync(ClipboardEntry entry);

    /// <summary>
    /// Get all clipboard entries ordered by most recent first.
    /// </summary>
    Task<List<ClipboardEntry>> GetEntriesAsync(int limit = 50);

    /// <summary>
    /// Delete a specific entry.
    /// </summary>
    Task DeleteEntryAsync(int id);

    /// <summary>
    /// Delete all non-pinned entries.
    /// </summary>
    Task ClearAllAsync();

    /// <summary>
    /// Toggle pin status for an entry.
    /// </summary>
    Task TogglePinAsync(int id);

    /// <summary>
    /// Check if content already exists (by hash).
    /// </summary>
    Task<ClipboardEntry?> FindByHashAsync(string hash);

    /// <summary>
    /// Move existing entry to top (update timestamp).
    /// </summary>
    Task MoveToTopAsync(int id);

    /// <summary>
    /// Enforce history limit by removing oldest non-pinned entries.
    /// </summary>
    Task EnforceHistoryLimitAsync(int limit);
}
