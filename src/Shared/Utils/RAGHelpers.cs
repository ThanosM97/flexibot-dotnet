using System.Text;
using System.Text.RegularExpressions;
using Shared.Interfaces.AI.Language;
using Shared.Models;


namespace Shared.Utils;

/// <summary>
/// Provides helper methods for extracting and processing data from text input.
/// </summary>
partial class RAGHelpers
{

    /// <summary>
    /// A regular expression for matching confidence scores in a specific format.
    /// </summary>
    /// <returns>A <see cref="Regex"/> object for matching confidence scores.</returns>
    /// <remarks> Expected format: "[Confidence: X.XX]"
    [GeneratedRegex(@"^\[Confidence:\s*((?:0\.\d{2})|(?:1\.00))\]")]
    private static partial Regex ConfidenceRegex();

    /// <summary>
    /// Attempts to extract a confidence score from the provided text chunk.
    /// </summary>
    /// <param name="currentChunk">The current chunk of text to process.</param>
    /// <param name="buffer">A <see cref="StringBuilder"/> used to accumulate text until a confidence score is found.</param>
    /// <param name="confidence">Outputs the extracted confidence score if successful; otherwise, 0.</param>
    /// <param name="remainingContent">Outputs the content remaining after the confidence score header if successful; otherwise, an empty string.</param>
    /// <returns><c>true</c> if a confidence score was successfully extracted; otherwise, <c>false</c>.</returns>
    public static bool TryExtractConfidenceScore(
        string currentChunk,
        StringBuilder buffer,
        out float confidence,
        out string remainingContent)
    {
        // Initialize out variables
        confidence = 0;
        remainingContent = string.Empty;

        // Append new chunk to buffer
        buffer.Append(currentChunk);
        var fullContent = buffer.ToString();

        // Try to match the confidence header
        var match = ConfidenceRegex().Match(fullContent);

        if (!match.Success) return false;

        // Parse confidence score
        if (!float.TryParse(match.Groups[1].Value, out float parsedConfidence))
        {
            return false;
        }

        // Set confidence score
        confidence = parsedConfidence;

        // Calculate remaining content after confidence header
        var headerEnd = match.Index + match.Length;
        remainingContent = fullContent[headerEnd..];

        // Clear buffer after successful extraction
        buffer.Clear();
        return true;

    }
}