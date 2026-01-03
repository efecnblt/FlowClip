using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FlowClip.Helpers;

/// <summary>
/// Helper class for image operations.
/// </summary>
public static class ImageHelper
{
    private const int ThumbnailMaxSize = 150;

    /// <summary>
    /// Save a BitmapSource to file and return the path.
    /// </summary>
    public static string SaveImage(BitmapSource image, string imagesFolder)
    {
        var fileName = $"clip_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
        var filePath = Path.Combine(imagesFolder, fileName);

        using var fileStream = new FileStream(filePath, FileMode.Create);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        encoder.Save(fileStream);

        return filePath;
    }

    /// <summary>
    /// Create a thumbnail from an image file.
    /// </summary>
    public static BitmapSource? CreateThumbnail(string imagePath)
    {
        if (!File.Exists(imagePath))
            return null;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath);
            bitmap.DecodePixelWidth = ThumbnailMaxSize;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Create a thumbnail from a BitmapSource.
    /// </summary>
    public static BitmapSource CreateThumbnail(BitmapSource source)
    {
        double scale = Math.Min(
            (double)ThumbnailMaxSize / source.PixelWidth,
            (double)ThumbnailMaxSize / source.PixelHeight);

        if (scale >= 1)
        {
            source.Freeze();
            return source;
        }

        var scaled = new TransformedBitmap(source, new ScaleTransform(scale, scale));
        scaled.Freeze();
        return scaled;
    }

    /// <summary>
    /// Delete an image file if it exists.
    /// </summary>
    public static void DeleteImage(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return;

        try
        {
            if (File.Exists(imagePath))
                File.Delete(imagePath);
        }
        catch
        {
            // Ignore deletion errors
        }
    }

    /// <summary>
    /// Get image dimensions as a string.
    /// </summary>
    public static string GetImageDimensions(BitmapSource image)
    {
        return $"{image.PixelWidth}x{image.PixelHeight}";
    }

    /// <summary>
    /// Load a BitmapSource from file.
    /// </summary>
    public static BitmapSource? LoadImage(string imagePath)
    {
        if (!File.Exists(imagePath))
            return null;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch
        {
            return null;
        }
    }
}
