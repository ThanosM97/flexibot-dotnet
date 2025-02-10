namespace Shared.Events
{    
    /// <summary>  
    /// Represents an event that is triggered when a document has been uploaded.  
    /// </summary>  
    /// <param name="DocumentId">The unique identifier of the document.</param>  
    /// <param name="ObjectStorageKey">The key used to locate the document in object storage.</param>  
    /// <param name="FileName">The name of the document file that was uploaded.</param>  
    /// <param name="UploadedAt">The date and time when the document was uploaded.</param>
    public record DocumentUploadedEvent(
        string DocumentId,
        string ObjectStorageKey,
        string FileName,
        DateTime UploadedAt
    );
}
