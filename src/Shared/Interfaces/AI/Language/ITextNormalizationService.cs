    namespace Shared.Interfaces.AI.Language
    {
        /// <summary>
        /// Defines a service for normalizing text input by applying various linguistic processing steps
        /// such as punctuation removal, stop word filtering, and whitespace normalization.
        /// </summary>
        public interface ITextNormalizationService
        {
            /// <summary>
            /// Normalizes the specified text input.
            /// </summary>
            /// <param name="input">The text input to normalize.</param>
            /// <returns>A normalized version of the input text with reduced complexity for further processing.</returns>
            string Normalize(string input);
        }
    }
