using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using FlowClip.Data;
using FlowClip.Models;
using FlowClip.Services.Interfaces;

namespace FlowClip.Services;

/// <summary>
/// Service for managing application settings.
/// </summary>
public class SettingsService : ISettingsService
{
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "FlowClip";

    private readonly IServiceProvider _serviceProvider;
    private AppSettings? _cachedSettings;

    public SettingsService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public bool IsStartupEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<AppSettings> GetSettingsAsync()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlowClipDbContext>();

        _cachedSettings = await context.Settings.FirstOrDefaultAsync();

        if (_cachedSettings == null)
        {
            _cachedSettings = new AppSettings();
            context.Settings.Add(_cachedSettings);
            await context.SaveChangesAsync();
        }

        return _cachedSettings;
    }

    /// <inheritdoc/>
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlowClipDbContext>();

        var existing = await context.Settings.FirstOrDefaultAsync();
        if (existing != null)
        {
            existing.HistoryLimit = settings.HistoryLimit;
            existing.HotkeyModifiers = settings.HotkeyModifiers;
            existing.HotkeyKey = settings.HotkeyKey;
            existing.RunOnStartup = settings.RunOnStartup;
            existing.Theme = settings.Theme;
            existing.WidgetPositionX = settings.WidgetPositionX;
            existing.WidgetPositionY = settings.WidgetPositionY;
            existing.IsPanelExpanded = settings.IsPanelExpanded;
        }
        else
        {
            context.Settings.Add(settings);
        }

        await context.SaveChangesAsync();
        _cachedSettings = settings;

        // Update startup registration
        SetStartupEnabled(settings.RunOnStartup);
    }

    /// <inheritdoc/>
    public async Task SaveWidgetPositionAsync(double x, double y)
    {
        var settings = await GetSettingsAsync();
        settings.WidgetPositionX = x;
        settings.WidgetPositionY = y;
        await SaveSettingsAsync(settings);
    }

    /// <inheritdoc/>
    public async Task SavePanelStateAsync(bool isExpanded)
    {
        var settings = await GetSettingsAsync();
        settings.IsPanelExpanded = isExpanded;
        await SaveSettingsAsync(settings);
    }

    /// <inheritdoc/>
    public void SetStartupEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null) return;

            if (enabled)
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch
        {
            // Registry access may fail
        }
    }
}
