namespace Shared.Models;


/// <summary>
/// Represents the result of chat bot process.
/// </summary>
public record ChatBotResult
(
    /// <summary>
    /// Indicates whether this chunk is the final part of the chat bot result.
    /// </summary>
    bool IsFinalChunk,

    /// <summary>
    /// The answer produced by the chat bot process, or a chunk of it.
    /// </summary>
    string Answer,

    /// <summary>
    /// The confidence level of the answer (between 0.0 and 1.0).
    /// </summary>
    float Confidence,

    /// <summary>
    /// The source that generated the answer.
    /// </summary>
    string Source
);