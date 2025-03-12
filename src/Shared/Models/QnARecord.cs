namespace Shared.Models
{
    /// <summary>
    /// Represents a record in the QnA dataset, including a question, its embedding, and an answer.
    /// </summary>
    public class QnARecord
    {
        /// <summary>
        /// Gets or sets the unique identifier for the QnA record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the question associated with the QnA record.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// Gets or sets the embedding vector for the question, used for similarity search.
        /// </summary>
        public float[] QuestionEmbedding { get; set; }

        /// <summary>
        /// Gets or sets the answer associated with the question in the QnA record.
        /// </summary>
        public string Answer { get; set; }
    }
}