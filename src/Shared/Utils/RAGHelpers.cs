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

    /// <summary>
    /// Generates an answer from the given chat messages, parsing and handling confidence scores.
    /// </summary>
    /// <param name="generator">The chat service used to generate responses.</param>
    /// <param name="chatMessages">The list of chat messages, including system prompts and user input.</param>
    /// <param name="confidenceThreshold">The confidence threshold to determine if the answer is reliable.</param>
    /// <param name="defaultAnswer">The default answer to return if the confidence score is below the threshold or not found.</param>
    /// <returns>An asynchronous stream of tuples containing generated chunks, a boolean indicating if it's the last chunk, and
    /// confidence score for the response.</returns>
    public static async IAsyncEnumerable<(string, bool, float)> GenerateAnswerWithConfidenceAsync(
        IChatService generator, List<ChatCompletionMessage> chatMessages, float confidenceThreshold, string defaultAnswer)
    {
        // Flag to check if the confidence score has been parsed
        bool confidenceParsed = false;
        // Variable to store the parsed confidence score
        float confidenceScore = 0;
        // StringBuilder to accumulate parts of the response until the confidence score is extracted
        StringBuilder confidenceBuffer = new();

        await foreach (var (chunk, isLastChunk) in generator.CompleteChatAsync(chatMessages))
        {
            if (!confidenceParsed)
            {
                 // Attempt to extract confidence score from the current chunk
                var parsed = TryExtractConfidenceScore(chunk, confidenceBuffer, out float confidence, out string remainingContent);
                if (parsed)
                {
                    // Set the flag and store the confidence score
                    confidenceParsed = true;
                    confidenceScore = confidence;

                    // If the confidence score is below the threshold, yield a default response and break the loop
                    if (confidence < confidenceThreshold)
                    {
                        yield return (defaultAnswer, true, confidenceScore);
                        break;
                    }

                    // If there's remaining content after parsing, yield it along with the confidence score
                    if (!string.IsNullOrEmpty(remainingContent))
                    {
                        yield return (remainingContent, isLastChunk, confidenceScore);
                    }
                }
            }
            else
            {
                // If confidence score has already been parsed, yield the chunk as is
                yield return (chunk, isLastChunk, confidenceScore);
            }

            // If it's the last chunk and confidence score was not parsed, yield a default no response message
            if (isLastChunk && !confidenceParsed)
            {
                yield return (defaultAnswer, true, 0);
            }
        }
    }
}