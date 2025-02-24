using Microsoft.AspNetCore.Mvc;

using Shared.Events;
using Shared.Interfaces.Database;
using Shared.Interfaces.Storage;
using Shared.Models;
using Shared.Services;


namespace Api.Controllers;

/// <summary>
/// Controller for managing document-related operations such as uploading and checking status.
/// </summary>
[ApiController]
[Route("documents")]
public class DocumentsController(
    IStorageService storageService, IDocumentRepository documentRepository, RabbitMQPublisher publisher) : ControllerBase
{
    private readonly IStorageService _minioStorageService = storageService;
    private readonly IDocumentRepository _documentRepository = documentRepository;
    private readonly RabbitMQPublisher _publisher = publisher;

    /// <summary>
    /// Uploads a document and stores its metadata, then publishes an event to RabbitMQ.
    /// </summary>
    /// <param name="file">The file to be uploaded.</param>
    /// <param name="language">The language of the document, optional.</param>
    /// <param name="tags">Tags associated with the document, optional.</param>
    /// <returns>An IActionResult indicating the result of the operation.</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(
        IFormFile file,
        [FromForm] string language = "unknown",
        [FromForm] string tags = "")
    {
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file.");

        // Create document metadata.
        var documentMetadata = new DocumentMetadata
        {
            ObjectStorageKey = $"{Guid.NewGuid()}/{file.FileName}",
            FileName = file.FileName,
            ContentType = file.ContentType,
            Extension = Path.GetExtension(file.FileName),
            Size = file.Length / 1024.0,  // Size in KB
            UploadedAt = DateTime.UtcNow,
            Language = language?.Trim(),
            Tags = tags?.Trim()
        };

        // Upload file to storage and insert metadata into the database.
        await _minioStorageService.UploadFileAsync(documentMetadata.ObjectStorageKey, file);
        await _documentRepository.InsertDocumentAsync(documentMetadata);

        // Create a document uploaded event
        DocumentUploadedEvent documentUploadedEvent = new(
            DocumentId: documentMetadata.DocumentId,
            ObjectStorageKey: documentMetadata.ObjectStorageKey,
            FileName: documentMetadata.FileName,
            UploadedAt: documentMetadata.UploadedAt
        );

        // Publish the document uploaded event
        await _publisher.PublishAsync(documentUploadedEvent, "document_uploaded");

        // Return an Accepted response with the job ID.
        return Accepted(new { JobId = documentMetadata.DocumentId });
    }

    /// <summary>
    /// Retrieves the status of an uploaded document by its job ID.
    /// </summary>
    /// <param name="jobId">The ID of the job to check status for.</param>
    /// <returns>An IActionResult indicating the document's upload status.</returns>
    [HttpGet("{jobId}/status")]
    public async Task<IActionResult> GetUploadStatus(string jobId)
    {
        // Retrieve the document from the database using the job ID.
        var document = await _documentRepository.GetDocumentAsync(jobId);

        // Check if the document exists and return the appropriate status.
        if (document == null)
            return NotFound();

        if (document.Status == 2)
            return Ok(new { Status = "Completed" });

        if (document.Status == -1)
            return BadRequest(new { Status = "Failed" });

        return Accepted(new { Status = "Processing" });
    }
}