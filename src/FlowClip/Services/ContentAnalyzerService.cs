using System.Security.Cryptography;
using System.Text;
using FlowClip.Helpers;
using FlowClip.Models;
using FlowClip.Services.Interfaces;

namespace FlowClip.Services;

/// <summary>
/// Service for analyzing clipboard content to determine type.
/// </summary>
public class ContentAnalyzerService : IContentAnalyzerService
{
    private const int DefaultPreviewLength = 100;

    /// <inheritdoc/>
    public ContentAnalysisResult AnalyzeText(string text)
    {
        var contentType = ContentPatternMatcher.DetectContentType(text);
        string? colorHex = null;

        if (contentType == ClipboardContentType.Color)
        {
            colorHex = ContentPatternMatcher.ExtractColorHex(text);
        }

        return new ContentAnalysisResult
        {
            ContentType = contentType,
            ColorHex = colorHex,
            Preview = GeneratePreview(text),
            ContentHash = ComputeHash(text)
        };
    }

    /// <inheritdoc/>
    public string GeneratePreview(string content, int maxLength = DefaultPreviewLength)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        // Normalize whitespace
        var normalized = content
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Replace("\t", " ");

        // Collapse multiple spaces
        while (normalized.Contains("  "))
        {
            normalized = normalized.Replace("  ", " ");
        }

        normalized = normalized.Trim();

        if (normalized.Length <= maxLength)
            return normalized;

        return normalized[..maxLength].TrimEnd() + "...";
    }

    /// <inheritdoc/>
    public string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
