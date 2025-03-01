namespace Shared.Events
{
    /// <summary>
    /// Represents an event triggered when a chat response is streamed.
    /// </summary>
    /// <param name="JobId">The identifier for the job associated with the chat response.</param>
    /// <param name="Prompt">The content of the chunk of the response.</param>
    /// <param name="Done">Boolean flag to define the last chunk of the response.</param>
    /// <param name="ChunkTimestamp">The date and time when the chunk of the response was returned.</param>
    public record ChatResponseStreamedEvent(
        string JobId,
        string Chunk,
        bool Done,
        DateTime ChunkTimestamp
    );
}

