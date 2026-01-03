namespace FlowClip.Models;

/// <summary>
/// Represents the type of content stored in a clipboard entry.
/// </summary>
public enum ClipboardContentType
{
    /// <summary>
    /// Plain text content.
    /// </summary>
    Text,

    /// <summary>
    /// Code snippet (detected by patterns like function, class, etc.).
    /// </summary>
    Code,

    /// <summary>
    /// Color value (hex code like #FF5733).
    /// </summary>
    Color,

    /// <summary>
    /// Image content (stored as file path to saved image).
    /// </summary>
    Image,

    /// <summary>
    /// URL/web address.
    /// </summary>
    Url,

    /// <summary>
    /// Email address.
    /// </summary>
    Email
}
