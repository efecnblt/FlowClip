using FlowClip.Models;

namespace FlowClip.Services.Interfaces;

/// <summary>
/// Analysis result from content analyzer.
/// </summary>
public class ContentAnalysisResult
{
    public ClipboardContentType ContentType { get; init; }
    public string? ColorHex { get; init; }
    public string Preview { get; init; } = string.Empty;
    public string ContentHash { get; init; } = string.Empty;
}

/// <summary>
/// Service for analyzing clipboard content to determine type.
/// </summary>
public interface IContentAnalyzerService
{
    /// <summary>
    /// Analyze text content and determine its type.
    /// </summary>
    ContentAnalysisResult AnalyzeText(string text);

    /// <summary>
    /// Generate a preview string for display.
    /// </summary>
    string GeneratePreview(string content, int maxLength = 100);

    /// <summary>
    /// Compute a hash for duplicate detection.
    /// </summary>
    string ComputeHash(string content);
}
