using Shared.Models;

namespace Shared.Events
{
    /// <summary>
    /// Represents an event triggered when a chat prompt is initiated.
    /// </summary>
    /// <param name="JobId">The identifier for the job associated with the chat prompt.</param>
    /// <param name="Prompt">The content of the prompt.</param>
    /// <param name="History">The conversation history.</param>
    /// <param name="Timestamp">The date and time when the prompt was initiated.</param>
    public record ChatPromptedEvent(
        string JobId,
        string Prompt,
        List<ChatCompletionMessage> History,
        DateTime Timestamp
    );
}
