namespace Shared.Events
{    public record DocumentParsedEvent(
        string DocumentId,
        string ObjectStorageKey,
        string FileName,
        DateTime ParsedAt,
        string ParsedTextContent
    );
}
