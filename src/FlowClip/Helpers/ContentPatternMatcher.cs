using System.Text.RegularExpressions;
using FlowClip.Models;

namespace FlowClip.Helpers;

/// <summary>
/// Helper class for detecting content types using regex patterns.
/// </summary>
public static partial class ContentPatternMatcher
{
    // Color patterns - hex codes
    [GeneratedRegex(@"^#(?:[0-9a-fA-F]{3}){1,2}$|^#(?:[0-9a-fA-F]{4}){1,2}$", RegexOptions.Compiled)]
    private static partial Regex HexColorRegex();

    // RGB/RGBA patterns
    [GeneratedRegex(@"^rgba?\s*\(\s*\d{1,3}\s*,\s*\d{1,3}\s*,\s*\d{1,3}\s*(?:,\s*[\d.]+\s*)?\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex RgbColorRegex();

    // URL pattern
    [GeneratedRegex(@"^https?://[\w\-]+(\.[\w\-]+)+[/#?]?.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    // Email pattern
    [GeneratedRegex(@"^[\w\.\-]+@[\w\.\-]+\.\w{2,}$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    // Code patterns - common programming constructs
    [GeneratedRegex(@"^(?:\{|\[|function\s|public\s+(?:class|interface|enum|struct|record)|private\s+(?:class|interface)|protected\s+|internal\s+|def\s+\w+\s*\(|import\s+|from\s+\w+\s+import|const\s+\w+\s*=|let\s+\w+\s*=|var\s+\w+\s*=|export\s+(?:default\s+)?(?:function|class|const|let)|interface\s+\w+|namespace\s+|using\s+\w+|#include\s*<|<!DOCTYPE|<\?xml|<\?php|package\s+\w+|@interface\s+|class\s+\w+\s*(?:\(|:)|struct\s+\w+|enum\s+\w+|fn\s+\w+|impl\s+|trait\s+|module\s+)", RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex CodePatternRegex();

    // Additional code indicators
    [GeneratedRegex(@"(?:=>|->|\$\{|\{\{|<\/\w+>|\/>|===|!==|&&|\|\||::\w+|@\w+\()", RegexOptions.Compiled)]
    private static partial Regex CodeIndicatorRegex();

    /// <summary>
    /// Detect the content type of the given text.
    /// </summary>
    public static ClipboardContentType DetectContentType(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return ClipboardContentType.Text;

        content = content.Trim();

        // Check for hex color (must be exact match)
        if (HexColorRegex().IsMatch(content))
            return ClipboardContentType.Color;

        // Check for RGB/RGBA color
        if (RgbColorRegex().IsMatch(content))
            return ClipboardContentType.Color;

        // Check for URL (single line)
        if (!content.Contains('\n') && UrlRegex().IsMatch(content))
            return ClipboardContentType.Url;

        // Check for email (single line)
        if (!content.Contains('\n') && EmailRegex().IsMatch(content))
            return ClipboardContentType.Email;

        // Check for code patterns
        if (CodePatternRegex().IsMatch(content) || CodeIndicatorRegex().IsMatch(content))
            return ClipboardContentType.Code;

        // Check for multiple lines with common code structure
        if (LooksLikeCode(content))
            return ClipboardContentType.Code;

        return ClipboardContentType.Text;
    }

    /// <summary>
    /// Extract hex color value from content.
    /// </summary>
    public static string? ExtractColorHex(string content)
    {
        var match = HexColorRegex().Match(content.Trim());
        return match.Success ? match.Value.ToUpperInvariant() : null;
    }

    /// <summary>
    /// Additional heuristics to detect code.
    /// </summary>
    private static bool LooksLikeCode(string content)
    {
        var lines = content.Split('\n');
        if (lines.Length < 2) return false;

        int codeIndicators = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Check for common code patterns
            if (trimmed.EndsWith(';') || trimmed.EndsWith('{') || trimmed.EndsWith('}'))
                codeIndicators++;

            // Check for indentation with spaces/tabs followed by code
            if ((line.StartsWith("    ") || line.StartsWith("\t")) && trimmed.Length > 0)
                codeIndicators++;

            // Check for comment patterns
            if (trimmed.StartsWith("//") || trimmed.StartsWith("/*") || trimmed.StartsWith("*") || trimmed.StartsWith("#"))
                codeIndicators++;
        }

        // If more than 30% of lines look like code, classify as code
        return codeIndicators > lines.Length * 0.3;
    }
}
