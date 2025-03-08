using Microsoft.Extensions.Configuration;

using Shared.Interfaces.AI.Language;
using Shared.Interfaces.AI.RAG;
using Shared.Interfaces.Search;
using Shared.Models;
using Shared.Prompts;
using static Shared.Utils.RAGHelpers;


namespace Shared.Services.AI.RAG
{
    /// <summary>
    /// Provides an implementation for simple Retrieval-Augmented Generation (RAG) using
    /// a vector database service for retrieval, an embedding service for generating
    /// embeddings, and a chat service for generating responses. This class is configured
    /// using specified settings from an IConfiguration object.
    /// </summary>
    public class SimpleRAG : IRetrievalAugmentedGeneration
    {
        private readonly IVectorDatabaseService _retriever;
        private readonly IEmbeddingService _embedder;
        private readonly IChatService _generator;
        private readonly string _collectionName;
        private readonly int _topK;
        private readonly float _confidenceThreshold;
        private readonly string _defaultAnswer;

        /// <summary>
        /// Initializes a new instance of the SimpleRAG class.
        /// </summary>
        /// <param name="retriever">Service for retrieving relevant data chunks from a vector database.</param>
        /// <param name="embedder">Service for generating embeddings from text.</param>
        /// <param name="generator">Service for generating chat responses.</param>
        /// <param name="config">Configuration object for retrieving RAG settings.</param>
        public SimpleRAG(
            IVectorDatabaseService retriever,
            IEmbeddingService embedder,
            IChatService generator,
            IConfiguration config)
        {
            _retriever = retriever;
            _embedder = embedder;
            _generator = generator;

            // Retrieve RAG-specific configuration settings
            var ragConfig = config.GetSection("RAG");
            _collectionName = ragConfig["DOCUMENT_COLLECTION"] ?? throw new InvalidOperationException("RAG__DOCUMENT_COLLECTION is not set.");
            _topK = int.Parse(ragConfig["TOP_K"] ?? "5");
            _confidenceThreshold = float.Parse(ragConfig["CONFIDENCE_THRESHOLD"] ?? "0.7");
            _defaultAnswer = ragConfig["DEFAULT_ANSWER"] ?? "I don't know the answer to this question";
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<(string, bool, float)> GenerateAnswerAsync(
            List<ChatCompletionMessage> chatHistory, bool stream=true)
        {
            // Generate query embedding using the user prompt (last chat message)
            float[] embeddings = await _embedder.GenerateEmbeddingAsync(chatHistory.Last().Msg);

            // Retrieve relevant chunks
            IEnumerable<SearchResult> chunks = await _retriever.SearchAsync(
                _collectionName,
                embeddings,
                topK: _topK
            );

            // Build system instruction prompt
            string systemPrompt = BuildSystemPrompt(chunks);

            // Prepend system prompt to chatHistory
            List<ChatCompletionMessage> chatMessages = [
                new ChatCompletionMessage { Msg = systemPrompt, Role = ChatRole.System },
                .. chatHistory,
                new ChatCompletionMessage { Msg = RAGPrompts.RAGInstructionRepeat(), Role = ChatRole.System }  // Repeat instructions
            ];

            // Generate answer using the GenerateAnswerWithConfidenceAsync method for confidence parsing
            float confidenceScore = -1;
            await foreach (var (chunk, isLastChunk, confidence) in GenerateAnswerWithConfidenceAsync(
                _generator, chatMessages, _confidenceThreshold, _defaultAnswer))
            {
                confidenceScore = confidence;
                yield return (chunk, isLastChunk, confidence);
            }

            // No chunk was returned
            if (confidenceScore == -1) yield return (_defaultAnswer, true, 0);
        }

        /// <summary>
        /// Builds a system prompt string by formatting retrieved chunks of data.
        /// </summary>
        /// <param name="chunks">The collection of search results containing data chunks.</param>
        /// <returns>A formatted string representing the system prompt with context from the retrieved chunks.</returns>
        public static string BuildSystemPrompt(IEnumerable<SearchResult> chunks)
        {
            // Group the chunks by filename
            var groupedChunks = chunks.GroupBy(c => c.FileName);

            // Concatenate groups into a formatted string with indices and content
            string context = string.Join(
                "\n\n",
                groupedChunks.Select((g, i) =>
                    $"@@: [{i + 1}] File: {g.Key}\nContent: {string.Join("\n", g.Select(c => c.Content))}")
            );

            return RAGPrompts.RAGInstruction(context);
        }

    }
}