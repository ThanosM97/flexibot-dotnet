namespace Shared.Models;


/// <summary>
/// Represents the result of a Retrieval-Augmented Generation (RAG) process.
/// </summary>
public record RAGResult
(
    /// <summary>
    /// Indicates whether this chunk is the final part of the RAG result.
    /// </summary>
    bool IsFinalChunk,

    /// <summary>
    /// The answer produced by the RAG process, or a chunk of it.
    /// </summary>
    string Answer,

    /// <summary>
    /// The confidence level of the answer (between 0.0 and 1.0).
    /// </summary>
    float Confidence
);