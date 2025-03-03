using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OllamaSharp;
using OllamaSharp.Models.Exceptions;
using System.Text;

using Shared.Interfaces.AI.Language;
using Shared.Models;


namespace Shared.Services.AI.Language
{

    public class OllamaChatService :  OllamaService, IChatService
    {
        private readonly OllamaApiClient _client;
        public OllamaChatService(IConfiguration config)
        {
            IConfigurationSection ollamaConfig = config.GetSection("OLLAMA");

            // Validate the Ollama configuration
            if (string.IsNullOrWhiteSpace(ollamaConfig["ENDPOINT"]))
            {
                throw new OllamaException("Ollama endpoint env variable has not been set.");
            }

            if (string.IsNullOrWhiteSpace(ollamaConfig["LLM_MODEL"]))
            {
                throw new OllamaException("Ollama llm model env variable has not been set.");
            }

            // Initialize the Ollama client
            _client = new(ollamaConfig["ENDPOINT"], ollamaConfig["LLM_MODEL"]);

            // Pull the model
            PullModel(ollamaConfig["ENDPOINT"], ollamaConfig["LLM_MODEL"]);
        }


        /// <inheritdoc/>
        public async IAsyncEnumerable<(string, bool)> CompleteChatAsync(
            IEnumerable<ChatCompletionMessage> messages, bool stream = true)
        {
            // Build the prompt from the provided messages
            string prompt = BuildPrompt(messages);

            // Check if the response should be streamed
            if (stream)
            {
                // Stream the response
                await foreach (var token in _client.GenerateAsync(prompt))
                {
                    // Yield the response token
                    yield return (token.Response, token.Done);
                }
            }
            else
            {
                // Complete the response
                var response = await _client.CompleteAsync(prompt);
                yield return (response.Message.ToString(), true);
            }
        }

        /// <summary>
        /// Constructs a prompt string from a collection of chat completion messages.
        /// This prompt is typically used to generate or continue a conversation with an assistant.
        /// </summary>
        /// <param name="messages">
        /// An <see cref="IEnumerable{ChatCompletionMessage}"/> representing the sequence of messages
        /// exchanged in the chat so far.
        /// </param>
        /// <returns>
        /// A formatted <see cref="string"/> that concatenates each message in the format
        /// "[Role]: Message" and appends a marker indicating the assistant's turn to respond.
        /// </returns>
        private static string BuildPrompt(IEnumerable<ChatCompletionMessage> messages)
        {
            // Initialize a new StringBuilder instance to build the prompt
            StringBuilder prompt = new();

            // Append each message in the chat sequence to the prompt
            foreach (ChatCompletionMessage message in messages)
            {
                prompt.Append($"[{message.Role}]: {message.Msg}\n");
            }

            // Append a marker indicating the assistant should now respond.
            prompt.Append($"[{Models.ChatRole.Assistant}]:");

            // Return the prompt as a string
            return prompt.ToString();
        }
    }
}