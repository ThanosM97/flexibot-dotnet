namespace Shared.Events
{
    /// <summary>
    /// Represents an event that is triggered when a document has been deleted.
    /// </summary>
    /// <param name="DocumentId">The unique identifier of the document.</param>
    /// <param name="DeletedAt">The date and time when the document was deleted.</param>
    public record DocumentDeletedEvent(
        string DocumentId,
        DateTime DeletedAt
    );
}