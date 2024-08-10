using Crates.Backend.Models;
using System.Text.Json;

namespace Crates.Backend.Services.helpers
{
    internal static class JsonDeserialiser
    {
        internal static async Task<DiscogsSearchResult> DeserializeSearchQuery(string jsonResponse)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return await Task.Run(() => JsonSerializer.Deserialize<DiscogsSearchResult>(jsonResponse, options));
        }
    }
}