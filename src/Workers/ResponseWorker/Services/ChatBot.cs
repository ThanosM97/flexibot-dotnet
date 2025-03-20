using Shared.Factories.AI.Language;
using Shared.Factories.AI.RAG;
using Shared.Interfaces.AI.Language;
using Shared.Interfaces.AI.RAG;
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
    /// <param name="storageService">The service used for handling storage operations.</param>
    /// <param name="config">The configuration settings for the ChatBot service.</param>
    public class ChatBot(IStorageService storageService, IConfiguration config)
    {
        private readonly IQnAService _qnaService = QnAFactory.GetQnAService(storageService, config);
        private readonly IRetrievalAugmentedGeneration _ragService = RAGFactory.GetRAGService(config);
        private readonly ITextNormalizationService _textNormalizationService = new TextNormalizationService();

        /// <summary>
        /// Asynchronously processes a chat history and user prompt to generate response chunks.
        /// </summary>
        /// <param name="history">The existing chat history as a list of messages.</param>
        /// <param name="prompt">The current user prompt to be processed.</param>
        /// <param name="stream">Indicates whether the response should be streamed.</param>
        /// <returns>An asynchronous stream of tuples containing the response chunk, a completion flag, and a confidence score.</returns>
        public async IAsyncEnumerable<ChatBotResult> CompleteChunkAsync(
            List<ChatCompletionMessage> history, string prompt, bool stream=true)
        {
            // Normalize the user prompt for QnA lookup
            string normalizedPrompt = _textNormalizationService.Normalize(prompt);

            // Try to get a response from QnA
            QnAResult qnaResult = await _qnaService.GetAnswerAsync(normalizedPrompt);

            // If a suitable answer was found, yield it as a final chunk
            if(qnaResult.Found)
            {
                yield return new ChatBotResult(true, qnaResult.Answer, qnaResult.Confidence, "QnA");
                yield break;
            }

            // Initialize the chat history with existing messages and add the user's current prompt
            List<ChatCompletionMessage> chat =
            [
                .. history ?? Enumerable.Empty<ChatCompletionMessage>(),
                new ChatCompletionMessage { Content = prompt, Role = ChatRole.User },
            ];

            // Yield chunks of the response
            await foreach(RAGResult ragResult in _ragService.GenerateAnswerAsync(chat, stream))
            {
                yield return new ChatBotResult(
                    ragResult.IsFinalChunk, ragResult.Answer, ragResult.Confidence, "RAG");
            }
        }
    }
}
