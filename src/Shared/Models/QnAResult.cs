namespace Shared.Models;


/// <summary>
/// Represents the result of a Question and Answer (QnA) process.
/// </summary>
public record QnAResult
(
    /// <summary>
    /// Indicates whether a suitable answer was found for the provided question.
    /// </summary>
    bool Found,

    /// <summary>
    /// The answer provided by the QnA system.
    /// </summary>
    string Answer,

    /// <summary>
    /// The confidence level of the answer (between 0.0 and 1.0).
    /// </summary>
    float Confidence
);