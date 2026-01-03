using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FlowClip.Data;
using FlowClip.Services;
using FlowClip.Services.Interfaces;
using FlowClip.ViewModels;

namespace FlowClip;

/// <summary>
/// Application entry point and dependency injection container.
/// </summary>
public partial class App : Application
{
    private static Mutex? _mutex;
    private IServiceProvider _serviceProvider = null!;

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public static IServiceProvider Services => ((App)Current)._serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Ensure single instance
        _mutex = new Mutex(true, "FlowClip_SingleInstance_Mutex", out bool isNewInstance);
        if (!isNewInstance)
        {
            MessageBox.Show("FlowClip is already running.", "FlowClip", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        base.OnStartup(e);

        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Initialize database
        InitializeDatabase();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddDbContext<FlowClipDbContext>();

        // Services
        services.AddSingleton<IClipboardMonitorService, ClipboardMonitorService>();
        services.AddSingleton<IClipboardDataService, ClipboardDataService>();
        services.AddSingleton<IContentAnalyzerService, ContentAnalyzerService>();
        services.AddSingleton<IHotkeyService, HotkeyService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ITrayIconService, TrayIconService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    private void InitializeDatabase()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlowClipDbContext>();
        context.Database.EnsureCreated();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
