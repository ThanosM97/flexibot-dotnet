namespace Shared.Prompts
{
    /// <summary>
    /// Provides static methods to generate RAG (Retrieval-Augmented Generation) prompt instructions
    /// with a specific response format and rules.
    /// </summary>
    public static class RAGPrompts
    {
        /// <summary>
        /// Generates the RAG instruction for repeating the response format and rules at the end of the prompt.
        /// </summary>
        /// <returns>
        /// A string containing the RAG instruction template with detailed response structure,
        /// context processing rules, and strict compliance items.
        /// </returns>
        public static string RAGInstructionRepeat() => """
            # MANDATORY RESPONSE HEADER
            Your response MUST ALWAYS begin with:
            "[Confidence: X.XX]" (X.XX = 0.00-1.00 float)

            # CONTEXT PROCESSING RULES
            - If answer isn't in context:
                - Header: "[Confidence: 0.00]"
                - Body: "No response."
            - If using context:
                1. Header shows calculated confidence score
                2. First line after header is answer text
                3. A source in context is delcared using the source declaration symbol "@@:"
                4. Do not include the source declaration symbol in your response
                5. Embed citations like [1][2] after relevant facts
                6. Final line: "Sources: [1]filenameX.extension, [2]filenameY.extension"

            # STRICT COMPLIANCE ITEMS
            - Header format EXACTLY "[Confidence: X.XX]"
            - Never reference confidence score in body text
            - System will strip header before client delivery
            - Never include the source declaration symbol

            # ANTI-REQUIREMENTS (STRICT PROHIBITIONS)
            - NO information beyond provided context
            - NO disclaimers about being an AI
            - NO explanations of your process
            - NO repeating conversation history
            """;

        /// <summary>
        /// Generates the RAG instruction for processing a given context and forming a response.
        /// </summary>
        /// <param name="context">The context to be used for forming the response.</param>
        /// <returns>
        /// A string containing the RAG instruction template with the required response structure,
        /// header implementation rules, and the provided context.
        /// </returns>
        public static string RAGInstruction(string context) => $$"""
            # REQUIRED RESPONSE STRUCTURE
            FIRST LINE MUST BE:
            [Confidence: X.XX]

            FOLLOWED BY:
            - Answer text with inline citations [1][2]
            - Final line: "Sources: [1]filenameX.extension, [2]filenameY.extension"

            PROVIDED CONTEXT:
            {{context}}

            # HEADER IMPLEMENTATION RULES
            - 0.00 confidence if any answer part is unsupported
            - Calculate confidence before writing response
            - Header exists even when confidence is 0.00
            - Client never sees "Confidence:" text (system-stripped)

            # CURRENT CONVERSATION HISTORY
            """;
    }
}
