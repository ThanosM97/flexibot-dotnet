using Microsoft.Extensions.Configuration;

using Shared.Interfaces.AI.Language;
using Shared.Services.AI.Language;


namespace Shared.Factories.AI.Language
{
    /// <summary>
    /// Factory class for creating chat service instances based on the specified provider.
    /// </summary>
    public static class ChatFactory
    {

        /// <summary>
        /// Creates an instance of <see cref="IChatService"/> based on the specified provider.
        /// </summary>
        /// <param name="config">The configuration settings required by the chat service.</param>
        /// <returns>An instance of <see cref="IChatService"/> corresponding to the specified provider.</returns>
        /// <exception cref="NotSupportedException">Thrown when an unsupported provider is specified.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the CHAT_PROVIDER configuration is not set.</exception>
        public static IChatService GetChatService(IConfiguration config)
        {
            // Get provider
            string provider = config["CHAT_PROVIDER"] ?? throw new InvalidOperationException("CHAT_PROVIDER has not been set.");

            return provider.ToLower() switch
            {
                "ollama" => new OllamaChatService(config),
                _ => throw new NotSupportedException($"Unsupported embedding provider: {provider}")
            };
        }
    }
}