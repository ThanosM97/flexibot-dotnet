namespace Shared.Events
{
    /// <summary>
    /// Represents an event that is triggered when a QnA file has been uploaded.
    /// </summary>
    /// <param name="FileName">The name of the QnA file that was uploaded.</param>
    /// <param name="UploadedAt">The date and time when the file was uploaded.</param>
    public record QnAUploadedEvent(
        string FileName,
        DateTime UploadedAt
    );
}
