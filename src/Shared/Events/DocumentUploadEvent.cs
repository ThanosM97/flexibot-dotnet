namespace Shared.Events
{    public record DocumentUploadedEvent(
        string DocumentId,
        string ObjectStorageKey,
        string FileName,
        DateTime UploadedAt
    );
}
