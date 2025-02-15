namespace Shared.Interfaces.AI.Language
{
    /// <summary>
    /// Interface for embedding services that provide methods to generate embeddings from input data.
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Asynchronously generates an embedding vector for a single input string.
        /// </summary>
        /// <param name="input">The input string for which to generate an embedding.</param>
        /// <returns>A task representing the asynchronous operation, with a float array result containing the embedding vector.</returns>
        Task<float[]> GenerateEmbeddingAsync(string input);

        /// <summary>
        /// Asynchronously generates embedding vectors for a collection of input strings.
        /// </summary>
        /// <param name="inputs">A collection of input strings for which to generate embeddings.</param>
        /// <returns>A task representing the asynchronous operation, with a 2D float array result containing
        /// the embedding vectors for each input.</returns>
        Task<float[][]> GenerateEmbeddingsAsync(IEnumerable<string> inputs);
    }
}
