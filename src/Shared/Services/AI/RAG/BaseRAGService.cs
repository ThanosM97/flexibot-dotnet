using Microsoft.Extensions.Configuration;
using System.Text;

using Shared.Interfaces.AI.Language;
using Shared.Interfaces.AI.RAG;
using Shared.Interfaces.Search;
using Shared.Models;
using Shared.Prompts;
using static Shared.Utils.RAGHelpers;


namespace Shared.Services.AI.RAG
{
    /// <summary>
    /// Provides an implementation for a base Retrieval-Augmented Generation (RAG) using
    /// a vector database service for retrieval, an embedding service for generating
    /// embeddings, and a chat service for generating responses. This class is configured
    /// using specified settings from an IConfiguration object.
    /// </summary>
    public abstract class BaseRAG : IRetrievalAugmentedGeneration
    {
        protected readonly IVectorDatabaseService _retriever;
        protected readonly IEmbeddingService _embedder;
        protected readonly IChatService _generator;
        protected readonly string _collectionName;
        protected readonly int _topK;
        protected readonly float _confidenceThreshold;
        protected readonly string _defaultAnswer;

        /// <summary>
        /// Initializes a new instance of the BaseRAG class.
        /// </summary>
        /// <param name="retriever">Service for retrieving relevant data chunks from a vector database.</param>
        /// <param name="embedder">Service for generating embeddings from text.</param>
        /// <param name="generator">Service for generating chat responses.</param>
        /// <param name="config">Configuration object for retrieving RAG settings.</param>
        protected BaseRAG(
            IVectorDatabaseService retriever,
            IEmbeddingService embedder,
            IChatService generator,
            IConfiguration config)
        {
            _retriever = retriever;
            _embedder = embedder;
            _generator = generator;

            // Retrieve RAG configuration settings
            var ragConfig = config.GetSection("RAG");
            _collectionName = config.GetSection("SEARCH")["DOCUMENT_COLLECTION"] ?? throw new InvalidOperationException(
                "SEARCH__DOCUMENT_COLLECTION is not set.");
            _topK = int.Parse(ragConfig["TOP_K"] ?? "5");
            _confidenceThreshold = float.Parse(ragConfig["CONFIDENCE_THRESHOLD"] ?? "0.7");
            _defaultAnswer = ragConfig["DEFAULT_ANSWER"] ?? "I don't know the answer to this question";
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<(string, bool, float)> GenerateAnswerAsync(
            List<ChatCompletionMessage> chatHistory, bool stream = true)
        {
            // Generate query vector
            float[] embeddings = await GetVectorQueryAsync(chatHistory);

            // Retrieve relevant chunks
            IEnumerable<SearchResult> chunks = await _retriever.SearchAsync(
                _collectionName,
                embeddings,
                topK: _topK
            );

            // Get chat messages
            List<ChatCompletionMessage> chatMessages = GetChatMessages(chunks, chatHistory);

            // Generate answer using the GenerateAnswerWithConfidenceAsync method for confidence parsing
            float confidenceScore = -1;
            await foreach (var (chunk, isLastChunk, confidence) in GenerateAnswerWithConfidenceAsync(
                chatMessages, _confidenceThreshold, _defaultAnswer))
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
        protected virtual string GetSystemPrompt(IEnumerable<SearchResult> chunks)
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

        /// <summary>
        /// Constructs a list of chat messages by integrating a system prompt derived from search result chunks
        /// and appending it to the existing chat history.
        /// </summary>
        /// <param name="chunks">A collection of <see cref="SearchResult"/> representing the search results to be
        /// included in the system prompt.</param>
        /// <param name="chatHistory">The existing list of <see cref="ChatCompletionMessage"/> representing the chat history.</param>
        /// <returns>
        /// A list of <see cref="ChatCompletionMessage"/> that includes the system prompt and repeated instructions,
        /// followed by the existing chat history.
        /// </returns>
        protected virtual List<ChatCompletionMessage> GetChatMessages(IEnumerable<SearchResult> chunks, List<ChatCompletionMessage> chatHistory)
        {
            // Build system instruction prompt
            string systemPrompt = GetSystemPrompt(chunks);

            // Prepend system prompt to chatHistory
            List<ChatCompletionMessage> chatMessages = [
                new ChatCompletionMessage { Msg = systemPrompt, Role = ChatRole.System },
                .. chatHistory,
                new ChatCompletionMessage { Msg = RAGPrompts.RAGInstructionRepeat(), Role = ChatRole.System }  // Repeat instructions
            ];

            return chatMessages;

        }

        /// <summary>
        /// Asynchronously generates a vector query from the provided chat history.
        /// </summary>
        /// <param name="chatHistory">A list of <see cref="ChatCompletionMessage"/> representing the chat history.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an array of floats,
        /// representing the generated vector query.
        /// </returns>
        protected abstract Task<float[]> GetVectorQueryAsync(List<ChatCompletionMessage> chatHistory);

        /// <summary>
        /// Generates an answer from the given chat messages, parsing and handling confidence scores.
        /// </summary>
        /// <param name="chatMessages">The list of chat messages, including system prompts and user input.</param>
        /// <param name="confidenceThreshold">The confidence threshold to determine if the answer is reliable.</param>
        /// <param name="defaultAnswer">The default answer to return if the confidence score is below the threshold or not found.</param>
        /// <returns>An asynchronous stream of tuples containing generated chunks, a boolean indicating if it's the last chunk, and
        /// confidence score for the response.</returns>
        protected virtual async IAsyncEnumerable<(string, bool, float)> GenerateAnswerWithConfidenceAsync(
            List<ChatCompletionMessage> chatMessages, float confidenceThreshold, string defaultAnswer)
        {
            // Flag to check if the confidence score has been parsed
            bool confidenceParsed = false;
            // Variable to store the parsed confidence score
            float confidenceScore = 0;
            // StringBuilder to accumulate parts of the response until the confidence score is extracted
            StringBuilder confidenceBuffer = new();

            await foreach (var (chunk, isLastChunk) in _generator.CompleteChatStreamAsync(chatMessages))
            {
                if (!confidenceParsed)
                {
                    // Attempt to extract confidence score from the current chunk
                    var parsed = TryExtractConfidenceScore(chunk, confidenceBuffer, out float confidence, out string remainingContent);
                    if (parsed)
                    {
                        // Set the flag and store the confidence score
                        confidenceParsed = true;
                        confidenceScore = confidence;

                        // If the confidence score is below the threshold, yield a default response and break the loop
                        if (confidence < confidenceThreshold)
                        {
                            yield return (defaultAnswer, true, confidenceScore);
                            break;
                        }

                        // If there's remaining content after parsing, yield it along with the confidence score
                        if (!string.IsNullOrEmpty(remainingContent))
                        {
                            yield return (remainingContent, isLastChunk, confidenceScore);
                        }
                    }
                }
                else
                {
                    // If confidence score has already been parsed, yield the chunk as is
                    yield return (chunk, isLastChunk, confidenceScore);
                }

                // If it's the last chunk and confidence score was not parsed, yield a default no response message
                if (isLastChunk && !confidenceParsed)
                {
                    yield return (defaultAnswer, true, 0);
                }
            }
        }
    }
}