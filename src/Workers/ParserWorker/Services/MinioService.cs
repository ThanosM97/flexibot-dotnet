using Minio;
using Minio.DataModel.Args;

namespace ParserWorker.Services
{
    /// <summary>
    /// Provides functionality to interact with a MinIO server, such as downloading files from a specified bucket.
    /// </summary>
    /// <remarks>
    /// This service encapsulates the creation and configuration of a <see cref="IMinioClient"/> using settings from
    /// the application configuration. It currently supports downloading a file from a pre-defined bucket ("documents").
    /// </remarks>
    public class MinioService
    {
        private readonly IMinioClient _minioClient;
        private const string BucketName = "documents";

        /// <summary>
        /// Initializes a new instance of the <see cref="MinioService"/> class.
        /// </summary>
        /// <param name="config">
        /// The application configuration containing the MinIO settings. It must have a "MINIO" section with the keys:
        /// <c>ENDPOINT</c>, <c>ACCESS_KEY</c>, and <c>SECRET_KEY</c>.
        /// </param>
        /// <remarks>
        /// The constructor reads the MinIO configuration from the provided <paramref name="config"/> and creates a
        /// <see cref="IMinioClient"/> instance which is used for interacting with the MinIO server.
        /// </remarks>
        public MinioService(IConfiguration config)
        {
            var minioConfig = config.GetSection("MINIO");
            _minioClient = new MinioClient()
                .WithEndpoint(minioConfig["ENDPOINT"])
                .WithCredentials(minioConfig["ACCESS_KEY"], minioConfig["SECRET_KEY"])
                .Build();
        }

        public async Task<Stream> DownloadFileAsync(string objectName)
        {
            var memoryStream = new MemoryStream();
            var args = new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));

            await _minioClient.GetObjectAsync(args);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
