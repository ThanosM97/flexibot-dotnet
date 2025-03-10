namespace Shared.Events
{
    /// <summary>
    /// Represents an event that is triggered when a QnA file has been deleted.
    /// </summary>
    /// <param name="FileName">The name of the QnA file that was deleted.</param>
    /// <param name="DeletedAt">The date and time when the file was deleted.</param>
    public record QnADeletedEvent(
        string FileName,
        DateTime DeletedAt
    );
}
