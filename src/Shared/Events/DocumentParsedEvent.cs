namespace Shared.Events
{
    /// <summary>
    /// Represents an event that is triggered when a document has been parsed.
    /// </summary>
    /// <param name="DocumentId">The unique identifier of the document.</param>
    /// <param name="ObjectStorageKey">The key used to locate the document in object storage.</param>
    /// <param name="FileName">The name of the document file that was parsed.</param>
    /// <param name="ParsedAt">The date and time when the document was parsed.</param>
    /// <param name="ParsedTextContent">The text content extracted from the parsed document.</param>
    public record DocumentParsedEvent(
        string DocumentId,
        string ObjectStorageKey,
        string FileName,
        DateTime ParsedAt,
        string ParsedTextContent
    );
}
