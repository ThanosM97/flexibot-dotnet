using CsvHelper;
using System.Globalization;

using Shared.Interfaces.AI.Language;
using Shared.Interfaces.Search;
using Shared.Interfaces.Storage;
using Shared.Models;
using Shared.Factories.AI.Language;
using Shared.Factories.Search;


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
        private readonly IStorageService _storageService = storageService;
        private readonly IEmbeddingService _embeddingService = EmbeddingFactory.GetEmbeddingService(config);
        private readonly IVectorDatabaseService _vectorDatabaseService = VectorDatabaseFactory.GetVectorDatabaseService(config);
        private readonly IConfiguration _config = config;
        private const string BucketName = "qna";
        private const string CollectionName = "qna";

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
            // Create collection if it doesn't exist
            int vectorSize = int.Parse(_config.GetSection("SEARCH")["VECTOR_SIZE"] ?? "384");
            await _vectorDatabaseService.CreateCollectionIfNotExistsAsync(CollectionName, vectorSize);

            // Get CSV filestream from storage
            var fileStream = await _storageService.DownloadFileAsync(fileName, BucketName);

            // Read the CSV file stream
            using var csv = new CsvReader(new StreamReader(fileStream), CultureInfo.InvariantCulture);

            // Get records from CSV
            List<QnARecord> records = [.. csv.GetRecords<QnARecord>()];

            // Generate embeddings for questions in parallel
            var tasks = records.Select(record => Task.Run(async () =>
            {
                record.QuestionEmbedding = await _embeddingService.GenerateEmbeddingAsync(record.Question);
            }));
            await Task.WhenAll(tasks);

            // Clear the vector database
            await _vectorDatabaseService.DeleteAllPointsAsync(CollectionName);

            // Store new embeddings in the vector database
            await _vectorDatabaseService.UpsertQnAVectorsAsync(CollectionName, records);
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
            // Clear the vector database
            await _vectorDatabaseService.DeleteAllPointsAsync(CollectionName);

            // Delete the CSV file from storage
            await _storageService.DeleteFileAsync(fileName, BucketName);
        }

    }
}
