using Shared.Models;


namespace Shared.Interfaces.AI.Language;

/// <summary>
/// Interface for QnA services that provide methods to process csv upload, deletion, and question answering.
/// </summary>
public interface IQnAService
{
    /// <summary>
    /// Processes CSV file and updates QnA knowledge base.
    /// </summary>
    /// <param name="filename">The csv filename to process.</param>
    Task ProcessQnAUploadAsync(string filename);

    /// <summary>
    /// Deletes the active QnA cache.
    /// </summary>
    /// <param name="filename">The name of the file to delete from storage.</param>
    Task DeleteQnACacheAsync(string filename);

    /// <summary>
    /// Get an answer from the QnA cache
    /// </summary>
    /// <param name="question">The question to ask the QnA cache.</param>
    Task<QnAResult> GetAnswerAsync(string question);
}