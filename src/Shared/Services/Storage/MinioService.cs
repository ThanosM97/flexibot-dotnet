using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

using Shared.Interfaces.Storage;


namespace Shared.Services.Storage
{
    /// <summary>
    /// Service for handling file storage operations with MinIO.
    /// </summary>
    public class MinioService : IStorageService
    {
        private readonly IMinioClient _minioClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinioService"/> class.
        /// </summary>
        /// <param name="config">The configuration object to get MinIO settings.</param>
        public MinioService(IConfiguration config)
        {
            // Retrieve MinIO configuration settings
            var minioConfig = config.GetSection("MINIO");

            // Initialize the MinIO client
            _minioClient = new MinioClient()
                .WithEndpoint(minioConfig["ENDPOINT"])
                .WithCredentials(minioConfig["ACCESS_KEY"], minioConfig["SECRET_KEY"])
                .Build();
        }


        /// <inheritdoc/>
        public async Task UploadFileAsync(string objectKey, IFormFile file, string bucketName = "documents")
        {
            // Open a read stream
            using var stream = file.OpenReadStream();

            // Reset stream position
            stream.Position = 0;

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey)
                .WithStreamData(stream)
                .WithObjectSize(file.Length);

            // Upload the file to MinIO
            await _minioClient.PutObjectAsync(putObjectArgs);
        }


        /// <inheritdoc/>
        public async Task<Stream> DownloadFileAsync(string objectName, string bucketName = "documents")
        {
            var memoryStream = new MemoryStream();
            var args = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));

            // Download the object into the memory stream
            await _minioClient.GetObjectAsync(args);

            // Reset the stream position
            memoryStream.Position = 0;

            return memoryStream;
        }

        /// <inheritdoc/>
        public async Task DeleteFileAsync(string objectName, string bucketName = "documents")
        {
            var args = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);

            // Download the object into the memory stream
            await _minioClient.RemoveObjectAsync(args);
        }
    }
}
