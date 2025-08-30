using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using MotoRental.Application.Interfaces;

namespace MotoRental.Infrastructure.Services
{
    public class StorageService(MinioClient minioClient, IConfiguration configuration, ILogger<StorageService> logger) : IStorageService
    {
        private readonly string _bucketName = configuration["MinIO:BucketName"] ?? "motorental";

        public async Task<string> UploadImageAsync(IFormFile file, string containerName = "cnh-images")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            if (file.ContentType != "image/png" && file.ContentType != "image/bmp")
                throw new InvalidOperationException("Invalid file format. Only PNG and BMP are allowed.");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("File size exceeds the maximum limit of 5MB.");

            try
            {
                var objectName = $"{containerName}/{Guid.NewGuid()}_{file.FileName}";

                using var stream = file.OpenReadStream();

                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(file.ContentType);

                await minioClient.PutObjectAsync(putObjectArgs);

                logger.LogInformation("File uploaded successfully: {ObjectName}", objectName);

                return objectName;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentException("Image URL cannot be null or empty");

            try
            {
                var objectName = imageUrl.Contains(_bucketName)
                    ? imageUrl.Split(new[] { _bucketName + "/" }, StringSplitOptions.None).Last()
                    : imageUrl;

                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName);

                await minioClient.RemoveObjectAsync(removeObjectArgs);

                logger.LogInformation("File deleted successfully: {ObjectName}", objectName);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting file {ImageUrl}", imageUrl);
                return false;
            }
        }
    }
}
