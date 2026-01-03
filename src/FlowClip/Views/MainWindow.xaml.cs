using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using FlowClip.Helpers.Win32;
using FlowClip.ViewModels;

namespace FlowClip.Views;

/// <summary>
/// Main window containing the floating widget and history panel.
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;
    private bool _isDragging;
    private Point _dragStartPoint;
    private DispatcherTimer? _topmostTimer;

    // Resize state
    private bool _isResizing;
    private Point _resizeStartPoint;
    private double _initialWidth;
    private double _initialHeight;

    // Saved panel size
    private double _panelWidth = 340;
    private double _panelHeight = 450;

    // Track window position for drag detection
    private double _dragStartWindowLeft;
    private double _dragStartWindowTop;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        Deactivated += MainWindow_Deactivated;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Get ViewModel from DI
        _viewModel = App.Services.GetRequiredService<MainViewModel>();
        DataContext = _viewModel;

        // Initialize ViewModel
        await _viewModel.InitializeAsync(this);

        // Restore panel state
        if (_viewModel.IsPanelExpanded)
        {
            ExpandPanel();
        }

        // Subscribe to panel expansion changes
        _viewModel.PropertyChanged += (s, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.IsPanelExpanded))
            {
                if (_viewModel.IsPanelExpanded)
                    ExpandPanel();
                else
                    CollapsePanel();
            }
        };

        // Ensure window stays on top with a timer
        _topmostTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _topmostTimer.Tick += (s, args) => EnsureTopmost();
        _topmostTimer.Start();
    }

    private void MainWindow_Deactivated(object? sender, EventArgs e)
    {
        // Re-apply topmost when window loses focus
        Dispatcher.BeginInvoke(() =>
        {
            Topmost = false;
            Topmost = true;
        }, DispatcherPriority.ApplicationIdle);
    }

    private void EnsureTopmost()
    {
        if (!Topmost)
        {
            Topmost = true;
        }
    }

    private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _topmostTimer?.Stop();

        // Save position before closing
        if (_viewModel != null)
        {
            await _viewModel.SavePositionAsync();
        }
    }

    #region Widget Dragging

    private void WidgetButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _dragStartPoint = e.GetPosition(this);
        _dragStartWindowLeft = Left;
        _dragStartWindowTop = Top;
        WidgetButton.CaptureMouse();
    }

    private void WidgetButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        WidgetButton.ReleaseMouseCapture();

        // Check if window actually moved (not just a click)
        var windowMoved = Math.Abs(Left - _dragStartWindowLeft) + Math.Abs(Top - _dragStartWindowTop);
        if (windowMoved < 5)
        {
            // Window didn't move, treat as click
            TogglePanel();
        }
        else
        {
            // Save position after drag
            _ = _viewModel?.SavePositionAsync();
        }
    }

    private void WidgetButton_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;

        var currentPoint = e.GetPosition(this);
        var offsetX = currentPoint.X - _dragStartPoint.X;
        var offsetY = currentPoint.Y - _dragStartPoint.Y;

        Left += offsetX;
        Top += offsetY;

        // Update ViewModel
        if (_viewModel != null)
        {
            _viewModel.WindowLeft = Left;
            _viewModel.WindowTop = Top;
        }
    }

    #endregion

    #region Panel Animation

    private void TogglePanel()
    {
        if (_viewModel != null)
        {
            _viewModel.IsPanelExpanded = !_viewModel.IsPanelExpanded;
        }
    }

    private void ExpandPanel()
    {
        var duration = TimeSpan.FromMilliseconds(200);
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        // Switch content visibility
        CollapsedIcon.Visibility = Visibility.Collapsed;
        ExpandedContent.Visibility = Visibility.Visible;

        // Change to dark background for expanded state
        WidgetButton.Background = (System.Windows.Media.Brush)FindResource("AcrylicBackgroundBrush");

        // Animate widget width
        WidgetButton.BeginAnimation(WidthProperty, null);
        var widthAnim = new DoubleAnimation(48, _panelWidth, duration) { EasingFunction = ease };
        WidgetButton.BeginAnimation(WidthProperty, widthAnim);

        // Show panel with explicit dimensions
        HistoryPanel.Visibility = Visibility.Visible;
        HistoryPanel.Width = _panelWidth;
        HistoryPanel.Height = _panelHeight;

        // Ensure dimensions are in sync
        WidgetButton.Width = _panelWidth;
    }

    private void CollapsePanel()
    {
        // Save current size before collapsing (prefer explicit dimensions over actual)
        if (!double.IsNaN(HistoryPanel.Width) && HistoryPanel.Width > 0)
            _panelWidth = HistoryPanel.Width;
        else if (HistoryPanel.ActualWidth > 0)
            _panelWidth = HistoryPanel.ActualWidth;

        if (!double.IsNaN(HistoryPanel.Height) && HistoryPanel.Height > 0)
            _panelHeight = HistoryPanel.Height;
        else if (HistoryPanel.ActualHeight > 0)
            _panelHeight = HistoryPanel.ActualHeight;

        var duration = TimeSpan.FromMilliseconds(150);
        var ease = new CubicEase { EasingMode = EasingMode.EaseIn };

        // Hide panel immediately
        HistoryPanel.Visibility = Visibility.Collapsed;

        // Animate widget width back to circle
        var widthAnim = new DoubleAnimation(_panelWidth, 48, duration) { EasingFunction = ease };
        widthAnim.Completed += (s, e) =>
        {
            // After animation completes, switch content and reset background
            CollapsedIcon.Visibility = Visibility.Visible;
            ExpandedContent.Visibility = Visibility.Collapsed;
            WidgetButton.Background = (System.Windows.Media.Brush)FindResource("AccentBrush");

            // Clear animation to allow direct property setting
            WidgetButton.BeginAnimation(WidthProperty, null);
            WidgetButton.Width = 48;
        };
        WidgetButton.BeginAnimation(WidthProperty, widthAnim);
    }

    #endregion

    #region Entry Click

    private void Entry_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is ClipboardEntryViewModel entry)
        {
            _viewModel?.CopyEntryCommand.Execute(entry);
        }
    }

    #endregion

    #region Panel Resize

    private void ResizeGrip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement grip)
        {
            _isResizing = true;
            _resizeStartPoint = e.GetPosition(this);
            // Use the stored panel size (which might be larger than ActualHeight if content doesn't fill)
            _initialWidth = _panelWidth;
            _initialHeight = _panelHeight;
            grip.CaptureMouse();
            e.Handled = true;
        }
    }

    private void ResizeGrip_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement grip)
        {
            _isResizing = false;
            grip.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void ResizeGrip_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isResizing) return;

        var currentPoint = e.GetPosition(this);
        var deltaX = currentPoint.X - _resizeStartPoint.X;
        var deltaY = currentPoint.Y - _resizeStartPoint.Y;

        // Calculate new size with minimums
        var newWidth = Math.Max(280, _initialWidth + deltaX);
        var newHeight = Math.Max(200, _initialHeight + deltaY);

        // Apply new size to panel
        HistoryPanel.Width = newWidth;
        HistoryPanel.Height = newHeight;

        // Update widget button width to match
        WidgetButton.BeginAnimation(WidthProperty, null); // Clear animation
        WidgetButton.Width = newWidth;

        // Save for next expand
        _panelWidth = newWidth;
        _panelHeight = newHeight;

        e.Handled = true;
    }

    #endregion
}
