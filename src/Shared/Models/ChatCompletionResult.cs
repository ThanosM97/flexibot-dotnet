namespace Shared.Models;


/// <summary>
/// Represents the result of a chat completion operation.
/// </summary>
public record ChatCompletionResult
(
    /// <summary>
    /// Indicates whether this chunk is the final chunk of the chat completion.
    /// </summary>
    bool IsFinalChunk,

    /// <summary>
    /// The answer produced by the chat completion process, or a chunk of it.
    /// </summary>
    string Answer,

    /// <summary>
    /// The confidence level of the answer (between 0.0 and 1.0).
    /// </summary>
    float Confidence
);