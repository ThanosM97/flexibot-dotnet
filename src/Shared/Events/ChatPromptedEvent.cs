using Shared.Models;

namespace Shared.Events
{
    /// <summary>
    /// Represents an event triggered when a chat prompt is initiated.
    /// </summary>
    /// <param name="JobId">The identifier for the job associated with the chat prompt.</param>
    /// <param name="SessionId">The unique identifier for the chat session.</param>
    /// <param name="Prompt">The content of the prompt.</param>
    /// <param name="PastMessagesIncluded">The number of past messages in the chat session.</param>
    /// <param name="Timestamp">The date and time when the prompt was initiated.</param>
    public record ChatPromptedEvent(
        string JobId,
        string SessionId,
        string Prompt,
        int PastMessagesIncluded,
        DateTime Timestamp
    );
}
