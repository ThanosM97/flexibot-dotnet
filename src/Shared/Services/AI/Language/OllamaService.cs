using System.Text;
using System.Text.Json;

namespace Shared.Services.AI.Language
{
    /// <summary>
    /// Provides methods to interact with the Ollama AI service.
    /// </summary>
    public class OllamaService
    {
        /// <summary>
        /// Sends a request to the specified endpoint to pull a language model.
        /// </summary>
        /// <param name="endpoint">The base URL of the Ollama service endpoint.</param>
        /// <param name="modelName">The name of the model to be pulled.</param>
        /// <exception cref="Exception">Thrown if the HTTP request to pull the model fails.</exception>
        protected static void PullModel(string endpoint, string modelName)
        {
            // Create an instance of HttpClient for sending requests
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endpoint);

            // Create the request body with the model name
            var requestBody = new { model = modelName };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send a POST request to the /api/pull endpoint and wait for the response
            var response = httpClient.PostAsync("/api/pull", content).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to pull model {modelName}. HTTP {response.StatusCode}: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");
            }
        }
    }
}
