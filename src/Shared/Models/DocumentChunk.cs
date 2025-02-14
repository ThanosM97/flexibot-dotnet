namespace Shared.Models;

/// <summary>
/// Represents a chunk of a document, specifically designed for the chunking process.
/// </summary>
/// <remarks>
/// This model focuses on chunk content, the actual text extracted from the document.
/// </remarks>
public class DocumentChunk
{
    /// <summary>
    /// Gets or sets the unique identifier of the document chunk.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the identifier of the document to which this chunk belongs.
    /// </summary>
    public required string DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the content of the chunk, representing the text extracted from the document.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the embedding vector associated with the chunk, used for similarity searches.
    /// </summary>
    public float[] Embedding { get; set; } = [];
}