namespace Shared.Events
{
    /// <summary>
    /// Represents an event related to a document's status.
    /// </summary>
    /// <param name="DocumentId">The unique identifier of the document.</param>
    /// <param name="EventStatus">The status of the event, represented as an integer.</param>
    public record DocumentStatusEvent(
        string DocumentId,
        int EventStatus
    );

    /// <summary>
    /// Defines the various statuses a document can have.
    /// </summary>
    public enum DocumentStatus
    {
        Uploaded = 0,
        Parsed = 1,
        Chunked = 2,
        Embedded = 3,
        Indexed = 4,
        Deleted = 5,
        Failed = -1
    }
}
