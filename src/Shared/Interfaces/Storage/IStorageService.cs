using Microsoft.AspNetCore.Http;

namespace Shared.Interfaces.Storage
{
    /// <summary>
    /// Defines a contract for file storage services, providing methods for uploading and downloading files.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Uploads a file to the storage.
        /// </summary>
        /// <param name="objectKey">The key under which the file will be stored.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UploadFileAsync(string objectKey, IFormFile file);

        /// <summary>
        /// Downloads a file from the storage.
        /// </summary>
        /// <param name="objectName">The name of the object to download.</param>
        Task<Stream> DownloadFileAsync(string objectName);
    }
}
