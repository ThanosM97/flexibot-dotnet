using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
    /// <summary>
    /// Represents the metadata of a document stored in the database.
    /// </summary>
    [Table("documents")]
    public class DocumentMetadata
    {
        /// <summary>
        /// Gets or sets the unique identifier for the document.
        /// </summary>
        [Key]
        [Column("id")]
        public string DocumentId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the key used to store the document in object storage.
        /// </summary>
        [Column("object_storage_key")]
        public required string ObjectStorageKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        [Column("file_name")]
        public required string FileName { get; set; }

        /// <summary>
        /// Gets or sets the content type (MIME type) of the document.
        /// </summary>
        [Column("content_type")]
        public string? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the language of the document.
        /// </summary>
        [Column("language")]
        public string? Language { get; set; }

        /// <summary>
        /// Gets or sets the file extension of the document.
        /// </summary>
        [Column("extension")]
        public string? Extension { get; set; }

        /// <summary>
        /// Gets or sets the size of the document in bytes.
        /// </summary>
        [Column("size")]
        public double Size { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the document was uploaded.
        /// </summary>
        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the document, as a comma-separated string.
        /// </summary>
        [Column("tags")]
        public string? Tags { get; set; }

        /// <summary>
        /// Gets or sets the current status of the document.
        /// </summary>
        [Column("status")]
        public string Status { get; set; } = "Uploaded";  // Uploaded -> Processing -> Indexed/Failed

        /// <summary>
        /// Gets or sets the status code representing the document's current state.
        /// </summary>
        [Column("status_code")]
        public int StatusCode { get; set; } = 0;  // 0: Uploaded, 1: Processing, 2: Indexed, -1: Failed
    }
}