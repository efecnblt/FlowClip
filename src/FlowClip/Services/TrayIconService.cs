using System.Drawing;
using System.Windows;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using FlowClip.Services.Interfaces;

namespace FlowClip.Services;

/// <summary>
/// Service for managing the system tray icon.
/// </summary>
public class TrayIconService : ITrayIconService
{
    private TaskbarIcon? _trayIcon;

    /// <inheritdoc/>
    public event EventHandler? ShowRequested;

    /// <inheritdoc/>
    public event EventHandler? SettingsRequested;

    /// <inheritdoc/>
    public event EventHandler? ExitRequested;

    /// <inheritdoc/>
    public void Initialize()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _trayIcon = new TaskbarIcon
            {
                ToolTipText = "FlowClip - Clipboard Manager",
                NoLeftClickDelay = true,
                Icon = CreateDefaultIcon()
            };

            // Create context menu
            var contextMenu = new System.Windows.Controls.ContextMenu();

            var showItem = new System.Windows.Controls.MenuItem { Header = "Show FlowClip" };
            showItem.Click += (s, e) => ShowRequested?.Invoke(this, EventArgs.Empty);

            var settingsItem = new System.Windows.Controls.MenuItem { Header = "Settings" };
            settingsItem.Click += (s, e) => SettingsRequested?.Invoke(this, EventArgs.Empty);

            var separator = new System.Windows.Controls.Separator();

            var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
            exitItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);

            contextMenu.Items.Add(showItem);
            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(separator);
            contextMenu.Items.Add(exitItem);

            _trayIcon.ContextMenu = contextMenu;

            // Double-click to show
            _trayIcon.TrayLeftMouseUp += (s, e) => ShowRequested?.Invoke(this, EventArgs.Empty);
        });
    }

    /// <summary>
    /// Creates a simple default icon programmatically.
    /// </summary>
    private static Icon CreateDefaultIcon()
    {
        // Create a simple 16x16 icon with a clipboard symbol
        using var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);

        // Background circle
        using var brush = new SolidBrush(Color.FromArgb(0, 120, 212)); // Accent blue
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.FillEllipse(brush, 1, 1, 14, 14);

        // "C" letter for Clipboard
        using var font = new Font("Segoe UI", 8, System.Drawing.FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        graphics.DrawString("C", font, textBrush, new RectangleF(0, 0, 16, 16), format);

        var hIcon = bitmap.GetHicon();
        return Icon.FromHandle(hIcon);
    }

    /// <inheritdoc/>
    public void ShowNotification(string title, string message)
    {
        _trayIcon?.ShowNotification(title, message, NotificationIcon.Info);
    }

    /// <inheritdoc/>
    public void SetTooltip(string tooltip)
    {
        if (_trayIcon != null)
        {
            _trayIcon.ToolTipText = tooltip;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}
