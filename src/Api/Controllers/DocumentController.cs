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
    IStorageService storageService, IDatabaseService<DocumentMetadata> documentRepository, RabbitMQPublisher publisher) : ControllerBase
{
    private readonly IStorageService _minioStorageService = storageService;
    private readonly IDatabaseService<DocumentMetadata> _documentRepository = documentRepository;
    private readonly RabbitMQPublisher _publisher = publisher;

    /// <summary>
    /// Uploads documents and stores their metadata, then publishes events to RabbitMQ.
    /// </summary>
    /// <param name="files">The document files to be uploaded.</param>
    /// <param name="language">The language of the documents, optional.</param>
    /// <param name="tags">Tags associated with the documents, optional.</param>
    /// <returns>An IActionResult indicating the result of the operation.</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocuments(
        List<IFormFile> files,
        [FromForm] string language = "unknown",
        [FromForm] string tags = "")
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files provided.");

        // Initialize list for job ids
        List<string> jobIds = [];

        foreach (var file in files)
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
            await _documentRepository.InsertAsync(documentMetadata);

            // Create a document uploaded event
            DocumentUploadedEvent documentUploadedEvent = new(
                DocumentId: documentMetadata.DocumentId,
                ObjectStorageKey: documentMetadata.ObjectStorageKey,
                FileName: documentMetadata.FileName,
                UploadedAt: documentMetadata.UploadedAt
            );

            // Publish the document uploaded event
            await _publisher.PublishAsync(documentUploadedEvent, "document_uploaded");

            // Add the job ID to the list
            jobIds.Add(documentMetadata.DocumentId);
        }

        // Return an Accepted response with the job ID
        return Accepted(new { JobIds = jobIds });
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
            document = await _documentRepository.GetObjByIdAsync(jobId);
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
        var documents = await _documentRepository.ListAsync();

        // Check if there are any documents
        if (documents == null || documents.Count == 0)
        return NotFound(new { Message = "No documents found." });

        // Select specific fields to return from indexed documents
        var documentList = documents
            .Where(doc => doc.Status == (int)DocumentStatus.Indexed)
            .Select(doc => new
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
            document = await _documentRepository.GetObjByIdAsync(documentId);
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
            await _documentRepository.UpdateAsync(
                new Dictionary<string, object>
                {
                    { "Status", DocumentStatus.Deleted }
                },
                documentId
            );
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