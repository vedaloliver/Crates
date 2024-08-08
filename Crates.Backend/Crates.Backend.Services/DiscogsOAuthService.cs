using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class DiscogsOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _consumerKey;
    private readonly string _consumerSecret;
    private readonly string _callbackUrl;

    public DiscogsOAuthService(HttpClient httpClient, string consumerKey, string consumerSecret, string callbackUrl)
    {
        _httpClient = httpClient;
        _consumerKey = consumerKey;
        _consumerSecret = consumerSecret;
        _callbackUrl = callbackUrl;
    }

    public async Task<(string RequestToken, string RequestTokenSecret)> GetRequestTokenAsync()
    {
        var nonce = GenerateNonce();
        var timestamp = GenerateTimestamp();

        var parameters = new Dictionary<string, string>
        {
            {"oauth_consumer_key", _consumerKey},
            {"oauth_nonce", nonce},
            {"oauth_signature_method", "PLAINTEXT"},
            {"oauth_timestamp", timestamp},
            {"oauth_callback", _callbackUrl},
            {"oauth_signature", $"{_consumerSecret}&"}
        };

        var authorizationHeader = GenerateAuthorizationHeader(parameters);

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.discogs.com/oauth/request_token");
        request.Headers.Add("Authorization", authorizationHeader);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var responseParams = ParseQueryString(content);

        return (responseParams["oauth_token"], responseParams["oauth_token_secret"]);
    }

    public string GetAuthorizationUrl(string requestToken)
    {
        return $"https://discogs.com/oauth/authorize?oauth_token={requestToken}";
    }

    public async Task<(string AccessToken, string AccessTokenSecret)> GetAccessTokenAsync(string requestToken, string requestTokenSecret, string verifier)
    {
        var nonce = GenerateNonce();
        var timestamp = GenerateTimestamp();

        var parameters = new Dictionary<string, string>
        {
            {"oauth_consumer_key", _consumerKey},
            {"oauth_nonce", nonce},
            {"oauth_signature_method", "PLAINTEXT"},
            {"oauth_timestamp", timestamp},
            {"oauth_token", requestToken},
            {"oauth_verifier", verifier},
            {"oauth_signature", $"{_consumerSecret}&{requestTokenSecret}"}
        };

        var authorizationHeader = GenerateAuthorizationHeader(parameters);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.discogs.com/oauth/access_token");
        request.Headers.Add("Authorization", authorizationHeader);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var responseParams = ParseQueryString(content);

        return (responseParams["oauth_token"], responseParams["oauth_token_secret"]);
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
        var header = new StringBuilder("OAuth ");
        foreach (var param in parameters)
        {
            header.Append($"{Uri.EscapeDataString(param.Key)}=\"{Uri.EscapeDataString(param.Value)}\",");
        }
        return header.ToString().TrimEnd(',');
    }

    private Dictionary<string, string> ParseQueryString(string queryString)
    {
        var result = new Dictionary<string, string>();
        foreach (var param in queryString.Split('&'))
        {
            var parts = param.Split('=');
            if (parts.Length == 2)
            {
                result[parts[0]] = Uri.UnescapeDataString(parts[1]);
            }
        }
        return result;
    }
}