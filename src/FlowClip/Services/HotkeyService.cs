using System.Windows;
using System.Windows.Interop;
using FlowClip.Helpers.Win32;
using FlowClip.Services.Interfaces;

namespace FlowClip.Services;

/// <summary>
/// Service for managing global hotkeys.
/// </summary>
public class HotkeyService : IHotkeyService
{
    private const int HotkeyId = 9000;

    private HwndSource? _hwndSource;
    private IntPtr _hwnd;
    private bool _isRegistered;
    private int _currentModifiers;
    private int _currentKey;

    /// <inheritdoc/>
    public event EventHandler? HotkeyPressed;

    /// <inheritdoc/>
    public bool Register(Window window, int modifiers, int key)
    {
        var helper = new WindowInteropHelper(window);
        helper.EnsureHandle();
        _hwnd = helper.Handle;

        _hwndSource = HwndSource.FromHwnd(_hwnd);
        _hwndSource?.AddHook(WndProc);

        return RegisterHotkey(modifiers, key);
    }

    /// <inheritdoc/>
    public void Unregister()
    {
        if (_isRegistered)
        {
            NativeMethods.UnregisterHotKey(_hwnd, HotkeyId);
            _isRegistered = false;
        }

        _hwndSource?.RemoveHook(WndProc);
    }

    /// <inheritdoc/>
    public bool UpdateHotkey(int modifiers, int key)
    {
        if (_isRegistered)
        {
            NativeMethods.UnregisterHotKey(_hwnd, HotkeyId);
            _isRegistered = false;
        }

        return RegisterHotkey(modifiers, key);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Unregister();
    }

    private bool RegisterHotkey(int modifiers, int key)
    {
        // Add MOD_NOREPEAT to prevent repeated firing when key is held
        uint fsModifiers = (uint)modifiers | NativeMethods.MOD_NOREPEAT;

        if (NativeMethods.RegisterHotKey(_hwnd, HotkeyId, fsModifiers, (uint)key))
        {
            _isRegistered = true;
            _currentModifiers = modifiers;
            _currentKey = key;
            return true;
        }

        return false;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }
}
