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

    // Credit card pattern (basic)
    [GeneratedRegex(@"^\d{4}[\s\-]?\d{4}[\s\-]?\d{4}[\s\-]?\d{4}$", RegexOptions.Compiled)]
    private static partial Regex CreditCardRegex();

    // SSN pattern
    [GeneratedRegex(@"^\d{3}[\s\-]?\d{2}[\s\-]?\d{4}$", RegexOptions.Compiled)]
    private static partial Regex SsnRegex();

    /// <summary>
    /// Check if content appears to be sensitive data (password, credit card, etc.)
    /// </summary>
    public static bool IsSensitiveData(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        content = content.Trim();

        // Don't save very short clipboard content that might be passwords
        // Passwords are typically 8-64 characters, single line, no spaces
        if (!content.Contains('\n') && content.Length >= 8 && content.Length <= 128)
        {
            // Check if it looks like a password:
            // - Has mixed character types (upper, lower, digit, special)
            // - No spaces or very few spaces
            // - Not a common sentence structure

            bool hasUpper = content.Any(char.IsUpper);
            bool hasLower = content.Any(char.IsLower);
            bool hasDigit = content.Any(char.IsDigit);
            bool hasSpecial = content.Any(c => !char.IsLetterOrDigit(c) && c != ' ');
            int spaceCount = content.Count(c => c == ' ');

            // If it has 3+ character types and few spaces, likely a password
            int charTypeCount = (hasUpper ? 1 : 0) + (hasLower ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);
            if (charTypeCount >= 3 && spaceCount <= 1)
                return true;
        }

        // Check for credit card numbers
        if (CreditCardRegex().IsMatch(content))
            return true;

        // Check for SSN
        if (SsnRegex().IsMatch(content))
            return true;

        // Check for common password-related clipboard content from password managers
        if (content.Length <= 128 && !content.Contains('\n') &&
            (content.Contains("password", StringComparison.OrdinalIgnoreCase) ||
             content.Contains("secret", StringComparison.OrdinalIgnoreCase)))
            return false; // These are likely labels, not actual passwords

        return false;
    }

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
