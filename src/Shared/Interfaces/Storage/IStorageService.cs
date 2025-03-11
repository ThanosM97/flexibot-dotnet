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
        /// <param name="bucketName">The name of the bucket to upload the file to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UploadFileAsync(string objectKey, IFormFile file, string bucketName = "documents");

        /// <summary>
        /// Downloads a file from the storage.
        /// </summary>
        /// <param name="objectName">The name of the object to download.</param>
        /// <param name="bucketName">The name of the bucket to download the file from.</param>
        /// <returns>A task representing the asynchronous operation, containing the downloaded file as a stream.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the file cannot be downloaded.</exception>
        Task<Stream> DownloadFileAsync(string objectName, string bucketName = "documents");

        /// <summary>
        /// Deletes a file from the storage.
        /// </summary>
        /// <param name="objectName">The name of the object to delete.</param>
        /// <param name="bucketName">The name of the bucket to delete the file from.</param>
        /// <returns>A task representing the asynchronous operation, containing a boolean indicating
        /// whether the deletion was successful.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the file cannot be deleted.</exception>
        Task<bool> DeleteFileAsync(string objectName, string bucketName = "documents");
    }
}
