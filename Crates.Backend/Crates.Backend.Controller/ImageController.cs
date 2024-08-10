using Crates.Backend.Models;
using Crates.Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Crates.Backend.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService imageService;
        private readonly IDiscogsSearchService discogsService;

        public ImageController(IImageService imageService, IDiscogsSearchService discogsSearchService)
        {
            this.imageService = imageService;
            this.discogsService = discogsSearchService;
        }

        // Uploads the image to the blob,triggers OCR, and seraches it against Discogs APi
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            try
            {
                var imageUrl = await this.imageService.UploadImageAsync(file);
                //var ocrResult = await this.imageService.PerformOCRAsync(imageUrl);
                //return Ok(new { url = imageUrl, ocrText = ocrResult });
                return Ok(new { url = imageUrl });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        // Searches the discogs api
        [HttpGet("search")]
        public async Task<IActionResult> DiscogsSearch(string query)
        {

            try
            {
                DiscogsSearchResult searchQuery = await this.discogsService.SearchAsync(query);
                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}
