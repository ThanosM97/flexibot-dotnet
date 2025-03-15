using CsvHelper.Configuration;


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
        public required int Id { get; set; }

        /// <summary>
        /// Gets or sets the question associated with the QnA record.
        /// </summary>
        public required string Question { get; set; }

        /// <summary>
        /// Gets or sets the normalized question associated with the QnA record.
        /// </summary>
        public string? NormalizedQuestion { get; set; }

        /// <summary>
        /// Gets or sets the embedding vector for the question, used for similarity search.
        /// </summary>
        public float[]? QuestionEmbedding { get; set; }

        /// <summary>
        /// Gets or sets the answer associated with the question in the QnA record.
        /// </summary>
        public required string Answer { get; set; }
    }

    /// <summary>
    /// Provides custom CSV mapping for <see cref="QnARecord"/> objects to handle specific
    /// parsing and serialization behaviors, such as ignoring certain fields like
    /// <c>NormalizedQuestion</c> and <c>QuestionEmbedding</c>.
    /// </summary>
    public class QnARecordMap : ClassMap<QnARecord>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QnARecordMap"/> class.
        /// Configures the CSV mapping for <see cref="QnARecord"/>, mapping the key
        /// properties while ignoring fields that are not present in the CSV data.
        /// </summary>
        public QnARecordMap()
        {
            Map(m => m.Id);
            Map(m => m.Question);
            Map(m => m.NormalizedQuestion).Ignore();
            Map(m => m.QuestionEmbedding).Ignore();
            Map(m => m.Answer);

        }
    }
}