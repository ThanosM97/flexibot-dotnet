using System.Text.RegularExpressions;

using Shared.Interfaces.AI.Language;


namespace Shared.Services.AI.Language;

/// <summary>
/// Text normalization service for normalizing text input by applying various linguistic processing steps
/// such as punctuation removal, stop word filtering, and whitespace normalization.
/// </summary>
public partial class TextNormalizationService : ITextNormalizationService
    {
        [GeneratedRegex(@"[^\w\s]")]
        private static partial Regex PunctuationRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex ExtraSpacesRegex();

        // A sample list of common English stop words
        private static readonly HashSet<string> _stopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "the", "and", "or", "but", "to", "of", "for", "in", "on",
            "with", "as", "at", "by", "from", "could", "would", "please", "some"
        };

        /// <summary>
        /// Normalizes the specified text input by lowering case sensitivity, removing punctuation,
        /// eliminating common stop words, and replacing multiple spaces with a single space.
        /// </summary>
        /// <param name="input">The text input to normalize.</param>
        /// <returns>A normalized version of the input text with reduced complexity for further processing.</returns>
        public string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Convert to lowercase
            var normalized = input.ToLowerInvariant();

            // Remove punctuation using regular expressions
            normalized = PunctuationRegex().Replace(normalized, string.Empty);

            // Remove stop words
            normalized = RemoveStopWords(normalized);

            // Trim extra spaces
            normalized = ExtraSpacesRegex().Replace(normalized, " ").Trim();

            // TODO: Check if lemmatization would help. It can be useful, but it depends,
            // on the specific use case. It could hurt the performance if there is domain-specific jargon.

            return normalized;
        }


        private static string RemoveStopWords(string text)
        {
            // Split the text into words
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Filter out any words that are in the stop words list
            var filteredWords = words.Where(word => !_stopWords.Contains(word));

            // Return the text after removing stop words
            return string.Join(" ", filteredWords);
        }

}