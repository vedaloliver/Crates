using Crates.Backend.Models;
using System.Text.Json;

namespace Crates.Backend.Services.Helpers
{
    internal static class JsonDeserialiser
    {
        internal static async Task<SearchResult> DeserializeSearchQuery(string jsonResponse)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return await Task.Run(() => JsonSerializer.Deserialize<SearchResult>(jsonResponse, options));
        }
    }
}