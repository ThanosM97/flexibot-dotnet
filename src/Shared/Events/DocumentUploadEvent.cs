
namespace Shared.Events
{
    public class DocumentUploadedEvent
    {
        public string? FileName { get; set; }
        public string? ObjectStorageKey { get; set; }
    }
}
