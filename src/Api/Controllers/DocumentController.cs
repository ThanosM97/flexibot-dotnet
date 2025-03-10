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

        // Upload file to storage and insert metadata into the database
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

        // Return an Accepted response with the job ID
        return Accepted(new { JobId = documentMetadata.DocumentId });
    }

    /// <summary>
    /// Retrieves the status of an uploaded document by its job ID.
    /// </summary>
    /// <param name="jobId">The ID of the job to check status for.</param>
    /// <returns>An IActionResult indicating the document's upload status.</returns>
    [HttpGet("status/{jobId}")]
    public async Task<IActionResult> GetUploadStatus(string jobId)
    {
        DocumentMetadata document;
        // Try to retrieve the document from the database using the document ID
        try
        {
            document = await _documentRepository.GetDocumentAsync(jobId);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Document not found." });
        }

        if (document.Status == (int)DocumentStatus.Indexed)
            return Ok(new { Status = "Completed" });

        if (document.Status == (int)DocumentStatus.Failed)
            return BadRequest(new { Status = "Failed" });

        if (document.Status == (int)DocumentStatus.Deleted)
            return BadRequest(new { Status = "Deleted" });

        return Accepted(new { Status = "Processing" });
    }

    /// <summary>
    /// Retrieves a list of all documents' metadata.
    /// </summary>
    /// <returns>A list of documents' metadata.</returns>
    [HttpGet("list")]
    public async Task<IActionResult> GetDocumentsList()
    {
        // Retrieve the list of documents from the repository
        var documents = await _documentRepository.ListDocumentsAsync();

        // Check if there are any documents
        if (documents == null || documents.Count == 0)
        return NotFound(new { Message = "No documents found." });

        // Select specific fields to return
        var documentList = documents.Select(doc => new
        {
            doc.DocumentId,
            doc.FileName,
            doc.Extension,
            doc.Size
        });

        // Return the list of documents with specific fields
        return Ok(documentList);
    }

    /// <summary>
    /// Downloads a document by its document ID.
    /// </summary>
    /// <param name="documentId">The ID of the document to download.</param>
    /// <returns></returns>
    [HttpGet("download/{documentId}")]
    public async Task<IActionResult> DownloadDocument(string documentId)
    {
        DocumentMetadata document;
        // Try to retrieve the document from the database using the document ID
        try
        {
            document = await _documentRepository.GetDocumentAsync(documentId);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Document not found." });
        }

        // Download the file from storage
        var fileStream = await _minioStorageService.DownloadFileAsync(document.ObjectStorageKey);

        // Return the file as a stream
        return File(fileStream, document.ContentType, document.FileName);
    }

    /// <summary>
    /// Deletes a document from the knowledge base by its document ID.
    /// </summary>
    /// <param name="documentId">The ID of the document to delete.</param>
    /// <returns>An IActionResult indicating the result of the operation.</returns>
    [HttpDelete("delete/{documentId}")]
    public async Task<IActionResult> DeleteDocument(string documentId)
    {
        try
        {
            // Update the document status to Deleted
            await _documentRepository.UpdateDocumentAsync(documentId, new Dictionary<string, object>
            {
                { "Status", DocumentStatus.Deleted }
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Document not found." });
        }

        // Publish a document deleted event
        await _publisher.PublishAsync(
            new DocumentDeletedEvent(documentId, DateTime.UtcNow), "document_deleted");

        // Return a Ok response indicating the document was deleted
        return Ok();
    }
}