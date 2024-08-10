using Crates.Backend.Services;

using System.Text.Json;
using Crates.Backend.Models;
using Crates.Backend.Services.helpers;

public class DiscogsSearchService : IDiscogsSearchService
{
    private readonly HttpClient httpClient;
    private readonly string consumerKey;
    private readonly string consumerSecret;

    public DiscogsSearchService(HttpClient httpClient, string consumerKey, string consumerSecret)
    {
        this.httpClient = httpClient;
        this.consumerKey = consumerKey;
        this.consumerSecret = consumerSecret;
    }

    public async Task<DiscogsSearchResult> SearchAsync(string query)
    {
        var baseUrl = "https://api.discogs.com/database/search";

        // Add the consumer key and secret directly to the URL
        var requestUrl = $"{baseUrl}?q={Uri.EscapeDataString(query)}&key={Uri.EscapeDataString(consumerKey)}&secret={Uri.EscapeDataString(consumerSecret)}";

        // Create the HTTP request
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        // Add a User-Agent header to the request
        requestMessage.Headers.Add("User-Agent", "DiscogsSearchApp/1.0");

        var response = await this.httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync();
        return await JsonDeserialiser.DeserializeSearchQuery(jsonResponse);
    }
}
