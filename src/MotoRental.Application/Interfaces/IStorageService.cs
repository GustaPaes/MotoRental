using Microsoft.AspNetCore.Http;

namespace MotoRental.Application.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string containerName = "cnh-images");
        Task<bool> DeleteImageAsync(string imageUrl);
    }
}
