using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Text;
using ReadResult = Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.ReadResult;

namespace Crates.Backend.Services
{
    public class ImageService : IImageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ComputerVisionClient _computerVisionClient;

        public ImageService(BlobServiceClient blobServiceClient, ComputerVisionClient computerVisionClient)
        {
            _blobServiceClient = blobServiceClient;
            _computerVisionClient = computerVisionClient;
        }

        // Uploads the image to azure blob storage
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null", nameof(file));
            }

            string blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            string containerName = "images";
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Create the container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            // Return the URL of the uploaded blob
            return blobClient.Uri.ToString();
        }

        // Sends blob to azure OCR resource
        public async Task<string> PerformOCRAsync(string imageUrl)
        {
            var textHeaders = await _computerVisionClient.ReadAsync(imageUrl);
            string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);

            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            ReadOperationResult results;
            do
            {
                results = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));

            var stringBuilder = new StringBuilder();
            var textUrlFileResults = results.AnalyzeResult.ReadResults;

            foreach (ReadResult page in textUrlFileResults)
            {
                foreach (Line line in page.Lines)
                {
                    stringBuilder.AppendLine(line.Text.ToLower() + " ");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
