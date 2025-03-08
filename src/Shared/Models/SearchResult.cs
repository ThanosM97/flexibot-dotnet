namespace Shared.Models;


/// <summary>
/// Represents a search result containing information about a document chunk.
/// </summary>
public class SearchResult
{
    /// <summary>
    /// Gets or sets the unique identifier of the document chunk.
    /// </summary>
    public required string ChunkId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the document to which this chunk belongs.
    /// </summary>
    public required string DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the content of the chunk, representing the text extracted from the document.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the filename of the document to which this chunk belongs.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the confidence score of the search result.
    /// </summary>
    public required double ConfidenceScore { get; set; }
}