using Microsoft.AspNetCore.Mvc;

using Shared.Events;
using Shared.Interfaces.Storage;
using Shared.Services;


namespace Api.Controllers;

/// <summary>
/// Controller for managing QnA operations such as upload, download, and delete.
/// </summary>
[ApiController]
[Route("qna")]
public class QnAController(
    IStorageService storageService, RabbitMQPublisher publisher, IConfiguration config) : ControllerBase
{
    private readonly IStorageService _minioStorageService = storageService;
    private readonly RabbitMQPublisher _publisher = publisher;
    private readonly string _bucketName = config.GetSection("MINIO")["QNA_BUCKET"] ?? "qna";
    private const string FileName = "qna_knowledge_base.csv";

    /// <summary>
    /// Uploads a CSV file to the storage service.
    /// </summary>
    /// <param name="file">The CSV file to be uploaded.</param>
    /// <returns>Returns an accepted response if upload is successful, otherwise a bad request.</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadCsv(IFormFile file)
    {
        // Check if the file is null or empty
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file.");

        // Check if the file extension is .csv
        if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.CurrentCultureIgnoreCase))
            return BadRequest("Only CSV files are allowed.");

        // Upload file to storage and insert metadata into the database
        await _minioStorageService.UploadFileAsync(FileName, file, _bucketName);

        // Create a document uploaded event
        QnAUploadedEvent qnaUploadedEvent = new(
            FileName: FileName,
            UploadedAt: DateTime.UtcNow
        );

        // Publish the document uploaded event
        await _publisher.PublishAsync(qnaUploadedEvent, "qna_uploaded");

        // Return an Accepted response
        return Accepted();
    }

    /// <summary>
    /// Downloads the QnA CSV file from the storage service.
    /// </summary>
    /// <returns>Returns the file as a stream if it exists, otherwise a not found response.</returns>
    [HttpGet("download")]
    public async Task<IActionResult> DownloadQnA()
    {
        try
        {
            // Download the file from storage
            var fileStream = await _minioStorageService.DownloadFileAsync(FileName, _bucketName);

            // Return the file as a stream
            return File(fileStream, "text/csv", FileName);
        }
        catch (FileNotFoundException)
        {
            // If the file does not exist, return a 404 Not Found response
            return NotFound();
        }
        catch (InvalidOperationException)
        {
            // If the file cannot be downloaded, return a 500 Internal Server Error response
            return BadRequest("The file could not be downloaded.");
        }
    }

    /// <summary>
    /// Deletes the QnA CSV file from the storage service.
    /// </summary>
    /// <returns>Returns an OK response indicating the document was deleted.</returns>
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteQnA()
    {
        try
        {
            // Delete the file from storage
            bool deleted = await _minioStorageService.DeleteFileAsync(FileName, _bucketName);

            if (!deleted)
                return BadRequest("The file could not be deleted.");

            // Publish a document deleted event
            await _publisher.PublishAsync(
                new QnADeletedEvent(FileName, DateTime.UtcNow), "qna_deleted");

            // Return a Ok response indicating the document was deleted
            return Ok();
        }
        catch (FileNotFoundException)
        {
            // If the file does not exist, return a 404 Not Found response
            return NotFound();
        }
        catch (InvalidOperationException)
        {
            // If the file cannot be deleted, return a 500 Internal Server Error response
            return BadRequest("The file could not be deleted.");
        }
    }
}