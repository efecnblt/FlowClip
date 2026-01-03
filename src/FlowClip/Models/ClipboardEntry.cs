using System.ComponentModel.DataAnnotations;

namespace FlowClip.Models;

/// <summary>
/// Represents a single clipboard history entry.
/// </summary>
public class ClipboardEntry
{
    /// <summary>
    /// Unique identifier for the entry.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The actual content (text) or file path (for images).
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The detected type of content.
    /// </summary>
    public ClipboardContentType ContentType { get; set; }

    /// <summary>
    /// When the content was copied to clipboard.
    /// </summary>
    public DateTime CopiedAt { get; set; }

    /// <summary>
    /// Truncated preview for display (first ~100 chars).
    /// </summary>
    public string? Preview { get; set; }

    /// <summary>
    /// For Color type entries, stores the hex code (e.g., "#FF5733").
    /// </summary>
    public string? ColorHex { get; set; }

    /// <summary>
    /// For Image type entries, stores the path to the saved image file.
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Whether this entry is pinned (won't be auto-deleted).
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// Hash of content to detect duplicates.
    /// </summary>
    public string? ContentHash { get; set; }
}
