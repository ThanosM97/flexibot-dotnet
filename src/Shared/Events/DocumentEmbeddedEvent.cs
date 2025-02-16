using Shared.Models;

namespace Shared.Events
{
    /// <summary>
    /// Represents an event that is triggered when a document is chunked into smaller parts.
    /// </summary>
    /// <param name="DocumentId">The unique identifier for the document that has been chunked.</param>
    /// <param name="ObjectStorageKey">
    ///      The key or path in the object storage system to locate the original document or its representation.
    ///  </param>
    /// <param name="FileName">The name of the document file that was parsed.</param>
    /// <param name="Chunks">
    ///      A list of <see cref="DocumentChunk"/> objects representing the individual parts
    ///      into which the document has been divided.
    /// </param>
    /// <param name="EmbeddedAt">
    ///      The date and time when the document was embedded.
    ///  </param>
    public record DocumentEmbeddedEvent(
        string DocumentId,
        string ObjectStorageKey,
        string FileName,
        List<DocumentChunk> Chunks,
        DateTime EmbeddedAt
    );
}
