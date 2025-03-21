using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
    /// <summary>
    /// Represents a log entry in a chat system.
    /// </summary>
    [Table("chat_logs")]
    public class ChatLog
    {
        /// <summary>
        /// Gets or sets the unique identifier for the session.
        /// </summary>
        [Key]
        [Column("session_id")]
        public required string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the message.
        /// </summary>
        [Key]
        [Column("message_id")]
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the question asked in the chat.
        /// </summary>
        [Column("question")]
        public string? Question { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the request was made.
        /// </summary>
        [Column("request_timestamp")]
        public DateTime RequestTimestamp { get; set; }

        /// <summary>
        /// Gets the year extracted from the request timestamp.
        /// </summary>
        [Column("request_year")]
        public int RequestYear { get; }

        /// <summary>
        /// Gets the month extracted from the request timestamp.
        /// </summary>
        [Column("request_month")]
        public int RequestMonth { get; }

        /// <summary>
        /// Gets or sets the answer provided in the chat.
        /// </summary>
        [Column("answer")]
        public string? Answer { get; set; }

        /// <summary>
        /// Gets or sets the confidence score of the answer.
        /// </summary>
        [Column("confidence_score")]
        public decimal ConfidenceScore { get; set; }

        /// <summary>
        /// Gets or sets the source of the answer.
        /// </summary>
        [Column("source")]
        public string? Source { get; set; }
    }
}