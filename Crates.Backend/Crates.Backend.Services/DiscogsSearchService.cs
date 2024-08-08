using Crates.Backend.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

public class DiscogsSearchService : IDiscogsSearchService
{
    private readonly HttpClient httpClient;
    private readonly string consumerKey;
    private readonly string consumerSecret;
    private readonly string accessToken;
    private readonly string accessTokenSecret;

    public DiscogsSearchService(HttpClient httpClient, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
    {
        this.httpClient = httpClient;
        this.consumerKey = consumerKey;
        this.consumerSecret = consumerSecret;
        this.accessToken = accessToken;
        this.accessTokenSecret = accessTokenSecret;
    }

    public async Task<string> SearchAsync(string query)
    {
        var baseUrl = "https://api.discogs.com/database/search";
        var requestUrl = $"{baseUrl}?{query}";

        var nonce = GenerateNonce();
        var timestamp = GenerateTimestamp();

        var parameters = new Dictionary<string, string>
        {
            {"oauth_consumer_key", consumerKey},
            {"oauth_nonce", nonce},
            {"oauth_signature_method", "PLAINTEXT"},
            {"oauth_timestamp", timestamp},
            {"oauth_token", accessToken},
            {"oauth_signature", $"{consumerSecret}&{accessTokenSecret}"}
        };

        var authorizationHeader = GenerateAuthorizationHeader(parameters);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authorizationHeader);

        var response = await this.httpClient.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private string GenerateNonce()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=');
    }

    private string GenerateTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
    }

    private string GenerateAuthorizationHeader(Dictionary<string, string> parameters)
    {
        var header = new StringBuilder();
        foreach (var param in parameters)
        {
            header.Append($"{Uri.EscapeDataString(param.Key)}=\"{Uri.EscapeDataString(param.Value)}\",");
        }
        return header.ToString().TrimEnd(',');
    }
}