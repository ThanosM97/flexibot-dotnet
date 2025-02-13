using Shared.Models;
using System.Text;

namespace ChunkerWorker.Services
{
    /// <summary>
    /// Provides functionality to chunk a document into smaller parts.
    /// </summary>
    public class DocumentChunker
    {
        private readonly int _chunkSize;
        private readonly int _chunkOverlap;

        public DocumentChunker(IConfiguration config)
        {
            var chunkerConfig = config.GetSection("CHUNKS");

            // Try to parse the SIZE value, assign a default value of 1000 if parsing fails
            if (!int.TryParse(chunkerConfig["SIZE"], out _chunkSize))
            {
                _chunkSize = 1000;
            }

            // Try to parse the OVERLAP value, assign a default value of 200 if parsing fails
            if (!int.TryParse(chunkerConfig["OVERLAP"], out _chunkOverlap))
            {
                _chunkOverlap = 200;
            }
        }

        /// <summary>
        /// Chunks the given text into smaller parts based on predefined chunk size and overlap.
        /// </summary>
        /// <param name="text">The text to be chunked.</param>
        /// <param name="documentId">The identifier of the document being chunked.</param>
        /// <returns>A list of <see cref="DocumentChunk"/> representing the chunked parts of the document.</returns>
        public List<DocumentChunk> Chunk(string text, string documentId)
        {
            var chunks = new List<DocumentChunk>();
            var cleanText = NormalizeText(text);
            int start = 0;

            // Break characters
            char[] breakChars = [' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '-'];

            while (start < cleanText.Length)
            {
                // Get candidate end position
                int end = Math.Min(start + _chunkSize, cleanText.Length);

                if (end < cleanText.Length)
                {
                    // Avoid splitting mid-word
                    int lastBreak = cleanText.LastIndexOfAny(
                        breakChars, end-1, end-start);

                    if (lastBreak >= start)
                    {
                        end = lastBreak + 1;
                    }
                }

                // Add document chunk
                chunks.Add(new DocumentChunk
                {
                    DocumentId = documentId,
                    Content = cleanText[start..end]
                });

                // Get next start position
                start += end - _chunkOverlap <= start ? end : end - _chunkOverlap;
            }

            return chunks;
        }

        /// <summary>
        /// Normalizes the text by standardizing newlines, replacing tabs, removing replacement characters,
        /// and reducing double spaces.
        /// </summary>
        /// <param name="text">The text to normalize.</param>
        /// <returns>A normalized version of the text.</returns>
        private static string NormalizeText(string text)
        {
            // Basic text normalization
            var sb = new StringBuilder(text);
            sb.Replace("\r\n", "\n")      // Standardize newlines
            .Replace("\t", " ")         // Replace tabs
            .Replace("�", "")           // Remove replacement chars
            .Replace("  ", " ");        // Reduce double spaces

            return sb.ToString();
        }
    }
}

