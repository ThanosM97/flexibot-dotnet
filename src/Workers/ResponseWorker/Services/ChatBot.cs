using System.Text;

using Shared.Factories.AI.Language;
using Shared.Factories.AI.RAG;
using Shared.Interfaces.AI.Language;
using Shared.Interfaces.AI.RAG;
using Shared.Interfaces.Cache;
using Shared.Interfaces.Database;
using Shared.Interfaces.Storage;
using Shared.Models;
using Shared.Services.AI.Language;


namespace ResponseWorker.Services
{
    /// <summary>
    /// The ChatBot class provides functionality for generating responses to user prompts
    /// within a chat interface by utilizing QnA and Retrieval-Augmented Generation (RAG) services.
    /// </summary>
    /// <remarks>
    /// This class asynchronously processes user prompts and chat history to produce response
    /// chunks, leveraging both quick lookups via a QnA service and more comprehensive answer
    /// generation through a RAG service.
    /// </remarks>
    /// <param name="scopeFactory">The factory used for creating service scopes.</param>
    /// <param name="storageService">The service used for handling storage operations.</param>
    /// <param name="cacheService">The service used for handling caching operations.</param>
    /// <param name="config">The configuration settings for the ChatBot service.</param>
    public class ChatBot(
        IServiceScopeFactory scopeFactory,
        IStorageService storageService,
        ICacheService cacheService,
        IConfiguration config)
    {
        private readonly IQnAService _qnaService = QnAFactory.GetQnAService(storageService, config);
        private readonly IRetrievalAugmentedGeneration _ragService = RAGFactory.GetRAGService(config);
        private readonly ITextNormalizationService _textNormalizationService = new TextNormalizationService();
        private readonly ICacheService _cacheService = cacheService;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        /// <summary>
        /// Asynchronously processes a chat history and user prompt to generate response chunks.
        /// </summary>
        /// <param name="sessionId">The unique identifier for the current chat session.</param>
        /// <param name="prompt">The current user prompt to be processed.</param>
        /// <param name="pastMessages">The number of past messages to consider in generating the response.</param>
        /// <param name="stream">Indicates whether the response should be streamed.</param>
        /// <returns>An asynchronous stream of tuples containing the response chunk, a completion flag, and a confidence score.</returns>
        public async IAsyncEnumerable<ChatBotResult> CompleteChunkAsync(
            string sessionId, string prompt, int pastMessages = 10, bool stream=true)
        {
            // Create a new service scope to resolve scoped services
            using var scope = _scopeFactory.CreateScope();

            // Get the document repository service from the scope
            var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService<ChatLog>>();

            // Add the user prompt to the chat history in cache
            await CachePrompt(sessionId, prompt, databaseService);

            // Normalize the user prompt for QnA lookup
            string normalizedPrompt = _textNormalizationService.Normalize(prompt);

            // Try to get a response from QnA
            QnAResult qnaResult = await _qnaService.GetAnswerAsync(normalizedPrompt);

            // If a suitable answer was found, yield it as a final chunk
            if(qnaResult.Found)
            {
                // Cache the response and log the chat interaction
                await CacheAndLogChat(databaseService, sessionId, prompt, qnaResult.Answer, qnaResult.Confidence, "QnA");

                yield return new ChatBotResult(true, qnaResult.Answer, qnaResult.Confidence, "QnA");
                yield break;
            }

            // Get the chat history from cache
            List<ChatCompletionMessage> chat = await _cacheService.GetMessagesAsync(
                sessionId, pastMessages + 1); // +1 because the user prompt has been added to the chat history

            // Create a response builder
            StringBuilder responseBuilder = new();

            // Get confidence score
            bool isConfinceSet = false;
            float confidence = 0.0f;

            // Yield chunks of the response
            await foreach(RAGResult ragResult in _ragService.GenerateAnswerAsync(chat, stream))
            {
                // Append the response chunk to the builder
                responseBuilder.Append(ragResult.Answer);

                // Update the confidence score
                if (!isConfinceSet)
                {
                    confidence = ragResult.Confidence;
                    isConfinceSet = true;
                }

                yield return new ChatBotResult(
                    ragResult.IsFinalChunk, ragResult.Answer, ragResult.Confidence, "RAG");
            }

            // Get the complete resposne
            string response = responseBuilder.ToString();

            // Cache the response and log the chat interaction
            await CacheAndLogChat(databaseService, sessionId, prompt, response, confidence, "RAG");
        }

        /// <summary>
        /// Caches the user prompt in the chat history associated with the specified session ID.
        /// </summary>
        /// <param name="sessionId">The unique identifier for the session.</param>
        /// <param name="prompt">The user prompt to be cached.</param>
        /// <param name="databaseService">The database service used for retrieving and storing chat logs.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// If the session does not exist, a new session is created before caching the prompt.
        /// </remarks>
        private async Task CachePrompt(string sessionId, string prompt, IDatabaseService<ChatLog> databaseService)
        {
            // Create a new session if it doesn't exist
            if(!await _cacheService.SessionExistsAsync(sessionId))
            {
                // Get the chat logs from the database
                IEnumerable<ChatLog> chatLogs = await databaseService.GetObjByFilterAsync(
                    filter: e => e.SessionId == sessionId,
                    orderBy: q => q.OrderBy(e => e.RequestTimestamp)
                );

                // Rebuild the cache with the chat logs or create a new session
                if (chatLogs.Any())
                {
                    await _cacheService.RebuildCacheAsync(
                        sessionId,
                        chatLogs.SelectMany(chatLog => new[]
                        {
                            new ChatCompletionMessage
                            {
                                Role = ChatRole.User,
                                Content = chatLog.Question
                            },
                            new ChatCompletionMessage
                            {
                                Role = ChatRole.Assistant,
                                Content = chatLog.Answer
                            }
                        })
                    );
                }
                else
                {
                    await _cacheService.CreateSessionAsync(sessionId);
                }
            }

            // Add the user prompt to the chat history in cache
            await _cacheService.AppendMessageAsync(
                sessionId,
                new ChatCompletionMessage { Role = ChatRole.User, Content = prompt });
        }

        /// <summary>
        /// Caches the assistant's response and logs the chat interaction to the database.
        /// </summary>
        /// <param name="databaseService">The database service used for retrieving and storing chat logs.</param>
        /// <param name="sessionId">The unique identifier for the session.</param>
        /// <param name="prompt">The user prompt that initiated the response.</param>
        /// <param name="response">The assistant's response to the prompt.</param>
        /// <param name="confidence">The confidence score of the assistant's response.</param>
        /// <param name="source">The source of the response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method appends the assistant's response to the chat history cache and logs the interaction in the database.
        /// A new service scope is created to resolve scoped services required for database operations.
        /// </remarks>
        private async Task CacheAndLogChat(
            IDatabaseService<ChatLog> databaseService, string sessionId, string prompt, string response, float confidence, string source)
        {
            // Add the assistant's response to the chat history in cache
            await _cacheService.AppendMessageAsync(
                sessionId,
                new ChatCompletionMessage { Role = ChatRole.Assistant, Content = response });

            // Create a new chat log entry
            ChatLog chatLog = new()
            {
                SessionId = sessionId,
                Question = prompt,
                RequestTimestamp = DateTime.UtcNow,
                Answer = response,
                ConfidenceScore = (decimal)confidence,
                Source = source,
            };

            // Log the user prompt to the database
            await databaseService.InsertAsync(chatLog);
        }
    }
}
