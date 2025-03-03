using Microsoft.Extensions.Configuration;
using OllamaSharp;
using OllamaSharp.Models.Exceptions;

using Shared.Interfaces.AI.Language;


namespace Shared.Services.AI.Language
{
    /// <summary>
    /// Constructs the service using the OllamaSharp client.
    /// </summary>
    public class OllamaEmbeddingService :  OllamaService, IEmbeddingService
    {
        private readonly OllamaApiClient _client;
        public OllamaEmbeddingService(IConfiguration config)
        {
            IConfigurationSection ollamaConfig = config.GetSection("OLLAMA");

            // Validate the Ollama configuration
            if (string.IsNullOrWhiteSpace(ollamaConfig["ENDPOINT"]))
            {
                throw new OllamaException("Ollama endpoint env variable has not been set.");
            }

            if (string.IsNullOrWhiteSpace(ollamaConfig["EMBEDDING_MODEL"]))
            {
                throw new OllamaException("Ollama embedding model env variable has not been set.");
            }

            // Initialize the Ollama client
            _client = new(ollamaConfig["ENDPOINT"], ollamaConfig["EMBEDDING_MODEL"]);

            // Pull the model
            PullModel(ollamaConfig["ENDPOINT"], ollamaConfig["EMBEDDING_MODEL"]);
        }


        /// <summary>
        /// Generates an embedding vector for a single input string.
        /// </summary>
        public async Task<float[]> GenerateEmbeddingAsync(string input)
        {
            // Call the EmbedAsync method provided by OllamaSharp.
            var result = await _client.EmbedAsync(input);
            if (result == null || result.Embeddings == null || result.Embeddings.Count == 0)
            {
                throw new Exception("Failed to retrieve embedding from OllamaSharp.");
            }
            return result.Embeddings[0];
        }

        /// <summary>
        /// Generates embedding vectors for a collection of input strings.
        /// </summary>
        public async Task<float[][]> GenerateEmbeddingsAsync(IEnumerable<string> inputs)
        {
            var tasks = new List<Task<float[]>>();
            foreach (var input in inputs)
            {
                tasks.Add(GenerateEmbeddingAsync(input));
            }
            return await Task.WhenAll(tasks);
        }
    }
}