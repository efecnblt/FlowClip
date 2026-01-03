using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using FlowClip.Helpers.Win32;
using FlowClip.Services.Interfaces;

namespace FlowClip.Services;

/// <summary>
/// Service for monitoring system clipboard changes using Win32 API.
/// </summary>
public class ClipboardMonitorService : IClipboardMonitorService
{
    private HwndSource? _hwndSource;
    private IntPtr _hwnd;
    private bool _isMonitoring;
    private bool _isPaused;

    /// <inheritdoc/>
    public event EventHandler<ClipboardChangedEventArgs>? ClipboardChanged;

    /// <inheritdoc/>
    public bool IsPaused => _isPaused;

    /// <inheritdoc/>
    public void Start(Window window)
    {
        if (_isMonitoring) return;

        var helper = new WindowInteropHelper(window);
        helper.EnsureHandle();
        _hwnd = helper.Handle;

        _hwndSource = HwndSource.FromHwnd(_hwnd);
        _hwndSource?.AddHook(WndProc);

        if (NativeMethods.AddClipboardFormatListener(_hwnd))
        {
            _isMonitoring = true;
        }
    }

    /// <inheritdoc/>
    public void Stop()
    {
        if (!_isMonitoring) return;

        NativeMethods.RemoveClipboardFormatListener(_hwnd);
        _hwndSource?.RemoveHook(WndProc);
        _hwndSource = null;
        _isMonitoring = false;
    }

    /// <inheritdoc/>
    public void Pause()
    {
        _isPaused = true;
    }

    /// <inheritdoc/>
    public void Resume()
    {
        _isPaused = false;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Stop();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_CLIPBOARDUPDATE && !_isPaused)
        {
            OnClipboardChanged();
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void OnClipboardChanged()
    {
        // Read clipboard content on UI thread
        Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                string? textContent = null;
                BitmapSource? imageContent = null;

                // Try to get text first
                if (Clipboard.ContainsText())
                {
                    textContent = Clipboard.GetText();
                }
                // Then try image
                else if (Clipboard.ContainsImage())
                {
                    imageContent = Clipboard.GetImage();
                }

                // Only fire event if we got some content
                if (!string.IsNullOrEmpty(textContent) || imageContent != null)
                {
                    ClipboardChanged?.Invoke(this, new ClipboardChangedEventArgs(textContent, imageContent));
                }
            }
            catch (Exception)
            {
                // Clipboard access can fail if another app has it locked
                // Silently ignore these errors
            }
        });
    }
}
