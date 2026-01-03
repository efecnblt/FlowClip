using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowClip.Data;
using FlowClip.Helpers;
using FlowClip.Models;
using FlowClip.Services.Interfaces;

namespace FlowClip.ViewModels;

/// <summary>
/// Main ViewModel for the floating widget and history panel.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IClipboardMonitorService _clipboardMonitor;
    private readonly IClipboardDataService _dataService;
    private readonly IContentAnalyzerService _contentAnalyzer;
    private readonly ISettingsService _settingsService;
    private readonly ITrayIconService _trayIconService;
    private readonly IHotkeyService _hotkeyService;

    private AppSettings? _settings;

    [ObservableProperty]
    private bool _isPanelExpanded;

    [ObservableProperty]
    private double _windowLeft = 100;

    [ObservableProperty]
    private double _windowTop = 100;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isMonitoringPaused;

    public ObservableCollection<ClipboardEntryViewModel> Entries { get; } = [];

    public MainViewModel(
        IClipboardMonitorService clipboardMonitor,
        IClipboardDataService dataService,
        IContentAnalyzerService contentAnalyzer,
        ISettingsService settingsService,
        ITrayIconService trayIconService,
        IHotkeyService hotkeyService)
    {
        _clipboardMonitor = clipboardMonitor;
        _dataService = dataService;
        _contentAnalyzer = contentAnalyzer;
        _settingsService = settingsService;
        _trayIconService = trayIconService;
        _hotkeyService = hotkeyService;

        // Subscribe to clipboard changes
        _clipboardMonitor.ClipboardChanged += OnClipboardChanged;

        // Subscribe to tray icon events
        _trayIconService.ShowRequested += (s, e) => TogglePanel();
        _trayIconService.SettingsRequested += (s, e) => OpenSettingsCommand.Execute(null);
        _trayIconService.ExitRequested += (s, e) => ExitCommand.Execute(null);

        // Subscribe to hotkey
        _hotkeyService.HotkeyPressed += (s, e) => TogglePanel();
    }

    /// <summary>
    /// Initialize the ViewModel (load settings, history, etc.)
    /// </summary>
    public async Task InitializeAsync(Window window)
    {
        IsLoading = true;

        try
        {
            // Load settings
            _settings = await _settingsService.GetSettingsAsync();

            // Restore position
            if (_settings.WidgetPositionX.HasValue && _settings.WidgetPositionY.HasValue)
            {
                WindowLeft = _settings.WidgetPositionX.Value;
                WindowTop = _settings.WidgetPositionY.Value;
            }

            // Restore panel state
            IsPanelExpanded = _settings.IsPanelExpanded;

            // Load history
            await LoadEntriesAsync();

            // Start clipboard monitoring
            _clipboardMonitor.Start(window);

            // Register global hotkey
            _hotkeyService.Register(window, _settings.HotkeyModifiers, _settings.HotkeyKey);

            // Initialize tray icon
            _trayIconService.Initialize();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Save window position when moved.
    /// </summary>
    public async Task SavePositionAsync()
    {
        await _settingsService.SaveWidgetPositionAsync(WindowLeft, WindowTop);
    }

    [RelayCommand]
    private void TogglePanel()
    {
        IsPanelExpanded = !IsPanelExpanded;
        _ = _settingsService.SavePanelStateAsync(IsPanelExpanded);
    }

    [RelayCommand]
    private async Task CopyEntryAsync(ClipboardEntryViewModel? entry)
    {
        if (entry == null) return;

        // Pause monitoring to avoid re-adding the same content
        _clipboardMonitor.Pause();

        try
        {
            if (entry.ContentType == ClipboardContentType.Image && !string.IsNullOrEmpty(entry.ImagePath))
            {
                var image = ImageHelper.LoadImage(entry.ImagePath);
                if (image != null)
                {
                    Clipboard.SetImage(image);
                }
            }
            else
            {
                Clipboard.SetText(entry.Content);
            }

            // Move to top
            await _dataService.MoveToTopAsync(entry.Id);
            await LoadEntriesAsync();
        }
        finally
        {
            // Small delay before resuming to ensure clipboard operation completes
            await Task.Delay(100);
            _clipboardMonitor.Resume();
        }
    }

    [RelayCommand]
    private async Task DeleteEntryAsync(ClipboardEntryViewModel? entry)
    {
        if (entry == null) return;

        await _dataService.DeleteEntryAsync(entry.Id);
        Entries.Remove(entry);
    }

    [RelayCommand]
    private async Task TogglePinAsync(ClipboardEntryViewModel? entry)
    {
        if (entry == null) return;

        await _dataService.TogglePinAsync(entry.Id);
        entry.UpdatePinnedState(!entry.IsPinned);
        await LoadEntriesAsync();
    }

    [RelayCommand]
    private async Task ClearAllAsync()
    {
        await _dataService.ClearAllAsync();
        await LoadEntriesAsync();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        // TODO: Open settings window
        var settingsWindow = new Views.SettingsWindow();
        settingsWindow.ShowDialog();
    }

    [RelayCommand]
    private void TogglePauseMonitoring()
    {
        if (IsMonitoringPaused)
        {
            _clipboardMonitor.Resume();
            IsMonitoringPaused = false;
        }
        else
        {
            _clipboardMonitor.Pause();
            IsMonitoringPaused = true;
        }
    }

    [RelayCommand]
    private void Exit()
    {
        _clipboardMonitor.Stop();
        _hotkeyService.Unregister();
        Application.Current.Shutdown();
    }

    private async Task LoadEntriesAsync()
    {
        var limit = _settings?.HistoryLimit ?? 50;
        var entries = await _dataService.GetEntriesAsync(limit);

        Application.Current.Dispatcher.Invoke(() =>
        {
            Entries.Clear();
            foreach (var entry in entries)
            {
                Entries.Add(new ClipboardEntryViewModel(entry));
            }
        });
    }

    private async void OnClipboardChanged(object? sender, ClipboardChangedEventArgs e)
    {
        try
        {
            ClipboardEntry entry;

            if (e.HasText && !string.IsNullOrWhiteSpace(e.TextContent))
            {
                // Check for sensitive data (passwords, credit cards, etc.)
                if (ContentPatternMatcher.IsSensitiveData(e.TextContent))
                {
                    // Don't save sensitive data to history
                    return;
                }

                // Analyze text content
                var analysis = _contentAnalyzer.AnalyzeText(e.TextContent);

                // Check for duplicates
                var existing = await _dataService.FindByHashAsync(analysis.ContentHash);
                if (existing != null)
                {
                    // Move existing entry to top
                    await _dataService.MoveToTopAsync(existing.Id);
                    await LoadEntriesAsync();
                    return;
                }

                entry = new ClipboardEntry
                {
                    Content = e.TextContent,
                    ContentType = analysis.ContentType,
                    Preview = analysis.Preview,
                    ColorHex = analysis.ColorHex,
                    ContentHash = analysis.ContentHash
                };
            }
            else if (e.HasImage && e.ImageContent != null)
            {
                // Save image to file
                var imagePath = ImageHelper.SaveImage(e.ImageContent, FlowClipDbContext.ImagesPath);

                entry = new ClipboardEntry
                {
                    Content = imagePath,
                    ContentType = ClipboardContentType.Image,
                    Preview = $"Image ({ImageHelper.GetImageDimensions(e.ImageContent)})",
                    ImagePath = imagePath,
                    ContentHash = _contentAnalyzer.ComputeHash(imagePath + e.ImageContent.PixelWidth + e.ImageContent.PixelHeight)
                };
            }
            else
            {
                return;
            }

            // Add to database
            await _dataService.AddEntryAsync(entry);

            // Enforce history limit
            var limit = _settings?.HistoryLimit ?? 50;
            await _dataService.EnforceHistoryLimitAsync(limit);

            // Reload entries
            await LoadEntriesAsync();
        }
        catch (Exception)
        {
            // Log error but don't crash
        }
    }
}
