using Shared.Factories.AI.Language;
using Shared.Interfaces.AI.Language;
using Shared.Interfaces.Storage;


namespace QnAWorker.Services
{

    /// <summary>
    /// Represents a service that processes QnA records from a CSV file by generating embeddings and storing them in a vector database.
    /// </summary>
    /// <remarks>
    /// This service uses dependency injection to incorporate storage, embedding, and vector database services for processing QnA data.
    /// </remarks>
    public class QnAProcessor(IStorageService storageService, IConfiguration config)
    {
        private readonly IQnAService _qnaService = QnAFactory.GetQnAService(storageService, config);

        /// <summary>
        /// Processes the uploaded CSV file by reading QnA records, generating embeddings for each question,
        /// and storing them in the vector database.
        /// </summary>
        /// <param name="fileName">The name of the CSV file to process.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Downloads the CSV file from storage.
        /// 2. Reads the CSV file to extract QnA records.
        /// 3. Generates embeddings for each question in parallel.
        /// 4. Clears the QnA collection in the vector database.
        /// 5. Upserts the new embeddings into the vector database.
        /// </remarks>
        public async Task ProcessUploadAsync(string fileName)
        {
            // Process the uploaded file
            await _qnaService.ProcessQnAUploadAsync(fileName);
        }

        /// <summary>
        /// Clears the QnA knowledge base by deleting all vector data from the database and removing the
        /// associated CSV file from storage.
        /// </summary>
        /// <param name="fileName">The name of the CSV file to delete from storage.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Deletes all vector points from the specified collection in the vector database.
        /// 2. Removes the CSV file from the specified storage bucket.
        /// </remarks>
        public async Task ClearQnAKnowledgeBase(string fileName)
        {
            // Clear the knowledge base
            await _qnaService.DeleteQnACacheAsync(fileName);
        }

    }
}
