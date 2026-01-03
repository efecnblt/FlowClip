using System.Windows;

namespace FlowClip.Services.Interfaces;

/// <summary>
/// Event args for clipboard change events.
/// </summary>
public class ClipboardChangedEventArgs : EventArgs
{
    public string? TextContent { get; }
    public System.Windows.Media.Imaging.BitmapSource? ImageContent { get; }
    public bool HasText => TextContent != null;
    public bool HasImage => ImageContent != null;

    public ClipboardChangedEventArgs(string? text, System.Windows.Media.Imaging.BitmapSource? image)
    {
        TextContent = text;
        ImageContent = image;
    }
}

/// <summary>
/// Service for monitoring system clipboard changes.
/// </summary>
public interface IClipboardMonitorService : IDisposable
{
    /// <summary>
    /// Fired when clipboard content changes.
    /// </summary>
    event EventHandler<ClipboardChangedEventArgs>? ClipboardChanged;

    /// <summary>
    /// Start monitoring the clipboard.
    /// </summary>
    /// <param name="window">The window to receive clipboard messages.</param>
    void Start(Window window);

    /// <summary>
    /// Stop monitoring the clipboard.
    /// </summary>
    void Stop();

    /// <summary>
    /// Temporarily pause monitoring (e.g., when copying from history).
    /// </summary>
    void Pause();

    /// <summary>
    /// Resume monitoring after pause.
    /// </summary>
    void Resume();
}
