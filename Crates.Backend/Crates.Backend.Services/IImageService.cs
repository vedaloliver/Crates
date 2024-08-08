using Microsoft.AspNetCore.Http;

namespace Crates.Backend.Services
{
    // Interface for Image Service
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<string> PerformOCRAsync(string imageUrl);
    }
}
