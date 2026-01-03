using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FlowClip.Helpers;
using FlowClip.Models;

namespace FlowClip.ViewModels;

/// <summary>
/// ViewModel for a single clipboard entry in the history list.
/// </summary>
public partial class ClipboardEntryViewModel : ObservableObject
{
    private readonly ClipboardEntry _entry;

    [ObservableProperty]
    private bool _isPinned;

    [ObservableProperty]
    private BitmapSource? _thumbnail;

    public ClipboardEntryViewModel(ClipboardEntry entry)
    {
        _entry = entry;
        _isPinned = entry.IsPinned;

        // Load thumbnail for images
        if (entry.ContentType == ClipboardContentType.Image && !string.IsNullOrEmpty(entry.ImagePath))
        {
            _thumbnail = ImageHelper.CreateThumbnail(entry.ImagePath);
        }
    }

    public int Id => _entry.Id;
    public string Content => _entry.Content;
    public ClipboardContentType ContentType => _entry.ContentType;
    public DateTime CopiedAt => _entry.CopiedAt;
    public string Preview => _entry.Preview ?? _entry.Content;
    public string? ColorHex => _entry.ColorHex;
    public string? ImagePath => _entry.ImagePath;

    /// <summary>
    /// Gets the display text for the entry.
    /// </summary>
    public string DisplayText => ContentType switch
    {
        ClipboardContentType.Color => ColorHex ?? Content,
        ClipboardContentType.Image => $"Image ({ImageDimensions})",
        _ => Preview
    };

    /// <summary>
    /// Gets the type icon/label for the entry.
    /// </summary>
    public string TypeLabel => ContentType switch
    {
        ClipboardContentType.Code => "CODE",
        ClipboardContentType.Color => "COLOR",
        ClipboardContentType.Image => "IMAGE",
        ClipboardContentType.Url => "URL",
        ClipboardContentType.Email => "EMAIL",
        _ => ""
    };

    /// <summary>
    /// Gets whether this entry should show a type tag.
    /// </summary>
    public bool HasTypeTag => ContentType != ClipboardContentType.Text;

    /// <summary>
    /// Gets the color brush for color-type entries.
    /// </summary>
    public Brush? ColorBrush
    {
        get
        {
            if (ContentType != ClipboardContentType.Color || string.IsNullOrEmpty(ColorHex))
                return null;

            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(ColorHex));
            }
            catch
            {
                return Brushes.Transparent;
            }
        }
    }

    /// <summary>
    /// Gets the relative time string.
    /// </summary>
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.UtcNow - CopiedAt;

            if (diff.TotalSeconds < 60)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d ago";

            return CopiedAt.ToLocalTime().ToString("MMM d");
        }
    }

    /// <summary>
    /// Image dimensions string.
    /// </summary>
    private string ImageDimensions
    {
        get
        {
            if (Thumbnail != null)
                return $"{Thumbnail.PixelWidth}x{Thumbnail.PixelHeight}";
            return "?x?";
        }
    }

    /// <summary>
    /// Update the pinned state from model.
    /// </summary>
    public void UpdatePinnedState(bool isPinned)
    {
        IsPinned = isPinned;
        _entry.IsPinned = isPinned;
    }
}
