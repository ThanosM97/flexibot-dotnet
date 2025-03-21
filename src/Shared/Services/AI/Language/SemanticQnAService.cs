using CsvHelper;
using Microsoft.Extensions.Configuration;
using System.Globalization;

using Shared.Interfaces.AI.Language;
using Shared.Interfaces.Search;
using Shared.Interfaces.Storage;
using Shared.Models;


namespace Shared.Services.AI.Language;

/// <summary>
/// Provides services for managing and querying QnA data using semantic embedding similarity.
/// </summary>
/// <remarks>
/// This service utilizes vector databases and embedding services to perform semantic searches
/// and manage QnA data, allowing for efficient question-answer matching based on embedding
/// vector similarity.
/// </remarks>
/// <param name="vectorDbService">Service for interacting with the vector database.</param>
/// <param name="embeddingService">Service for generating embeddings for text data.</param>
/// <param name="storageService">Service for handling storage operations, such as file uploads and deletions.</param>
/// <param name="config">Configuration settings for the QnA service.</param>
public class SemanticQnAService(
    IVectorDatabaseService vectorDbService, IEmbeddingService embeddingService,
    IStorageService storageService, IConfiguration config
) : IQnAService
{
    private readonly IVectorDatabaseService _vectorDbService = vectorDbService;
    private readonly ITextNormalizationService _textNormalizationService = new TextNormalizationService();
    private readonly IEmbeddingService _embeddingService = embeddingService;
    private readonly IStorageService _storageService = storageService;
    private readonly int _vectorSize = int.Parse(config.GetSection("SEARCH")["VECTOR_SIZE"] ?? "384");
    private readonly string _collectionName = config.GetSection("SEARCH")["QNA_COLLECTION"] ?? "qna";
    private readonly string _defaultAnswer = config.GetSection("SEARCH")["DEFAULT_ANSWER"] ?? "I don't know the answer to this question.";
    private readonly float _confidenceThreshold = float.Parse(config.GetSection("QNA")["CONFIDENCE_THRESHOLD"] ?? "0.85");
    private readonly string _bucketName = config.GetSection("MINIO")["QNA_BUCKET"] ?? "qna";

    /// <inheritdoc/>
    public async Task ProcessQnAUploadAsync(string filename)
    {
        // Get CSV filestream from storage
        var fileStream = await _storageService.DownloadFileAsync(filename, _bucketName);

        // Read the CSV file stream
        using var csv = new CsvReader(new StreamReader(fileStream), CultureInfo.InvariantCulture);

        // Get records from CSV
        csv.Context.RegisterClassMap<QnARecordMap>();
        List<QnARecord> records = [.. csv.GetRecords<QnARecord>()];

        // Generate embeddings for questions in parallel
        var tasks = records.Select(record => Task.Run(async () =>
        {
            // Normalize question text
            string normalizedQuestion = _textNormalizationService.Normalize(record.Question);

            // Update record with normalized question
            record.NormalizedQuestion = normalizedQuestion;

            // Update record with normalized question embedding
            record.QuestionEmbedding = await _embeddingService.GenerateEmbeddingAsync(normalizedQuestion);
        }));
        await Task.WhenAll(tasks);

        // Create collection if it doesn't exist
        await _vectorDbService.CreateCollectionIfNotExistsAsync(_collectionName, _vectorSize);

        // Clear the vector database
        await _vectorDbService.DeleteAllPointsAsync(_collectionName);

        // Store new embeddings in the vector database
        await _vectorDbService.UpsertQnAVectorsAsync(_collectionName, records);
    }

    /// <inheritdoc/>
    public async Task DeleteQnACacheAsync(string filename)
    {
        // Clear the vector database
        await _vectorDbService.DeleteAllPointsAsync(_collectionName);

        // Delete the CSV file from storage
        await _storageService.DeleteFileAsync(filename, _bucketName);
    }

    /// <inheritdoc/>
    public async Task<QnAResult> GetAnswerAsync(string question)
    {
        // Normalize the question text
        string normalizedQuestion = _textNormalizationService.Normalize(question);

        // Generate embedding for the normalized question
        var questionEmbedding = await _embeddingService.GenerateEmbeddingAsync(normalizedQuestion);

        // Search for the closest question in the vector database
        IEnumerable<QnASearchResult> results = await _vectorDbService.SearchQnAAsync(_collectionName, questionEmbedding, 1);

        // Return the default answer if no results are found or if the confidence is below the threshold
        if (!results.Any() || results.First().ConfidenceScore < _confidenceThreshold)
            return new QnAResult(Found: false, Answer: _defaultAnswer, Confidence: 0.0f);

        // Return the answer
        return new QnAResult(
            Found: true,
            Answer: results.First().Answer,
            Confidence: results.First().ConfidenceScore
        );
    }
}