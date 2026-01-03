using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using FlowClip.Helpers.Win32;
using FlowClip.ViewModels;

namespace FlowClip.Views;

/// <summary>
/// Settings window.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow()
    {
        InitializeComponent();

        _viewModel = App.Services.GetRequiredService<SettingsViewModel>();
        DataContext = _viewModel;

        Loaded += SettingsWindow_Loaded;
        MouseLeftButtonDown += (s, e) => DragMove();
    }

    private async void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Apply acrylic effect
        try
        {
            WindowComposition.EnableAcrylic(this, System.Windows.Media.Color.FromArgb(0xE6, 0x1E, 0x1E, 0x1E));
        }
        catch
        {
            // Fallback: acrylic may not be supported
        }

        // Load settings
        await _viewModel.LoadAsync();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
