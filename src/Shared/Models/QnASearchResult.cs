namespace Shared.Models;


/// <summary>
/// Represents a search result containing information about a QnA record.
/// </summary>
public class QnASearchResult
{
    /// <summary>
    /// Gets or sets the identifier of the QnA record.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Gets or sets the question associated with the QnA record.
    /// </summary>
    public required string Question { get; set; }

    /// <summary>
    /// Gets or sets the answer associated with the QnA record.
    /// </summary>
    public required string Answer { get; set; }

    /// <summary>
    /// Gets or sets the confidence score of the search result.
    /// </summary>
    public required float ConfidenceScore { get; set; }
}