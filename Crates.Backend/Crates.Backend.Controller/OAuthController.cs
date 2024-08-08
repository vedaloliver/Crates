using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

[ApiController]
[Route("api/[controller]")]
public class OAuthController : ControllerBase
{
    private readonly DiscogsOAuthService _oauthService;
    private readonly IDistributedCache _cache;

    public OAuthController(DiscogsOAuthService oauthService, IDistributedCache cache)
    {
        _oauthService = oauthService;
        _cache = cache;
    }

    [HttpGet("start")]
    public async Task<IActionResult> StartOAuth()
    {
        var (requestToken, requestTokenSecret) = await _oauthService.GetRequestTokenAsync();

        // Store the requestTokenSecret securely in the distributed cache
        await _cache.SetStringAsync($"RequestTokenSecret_{requestToken}", requestTokenSecret, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        var authorizationUrl = _oauthService.GetAuthorizationUrl(requestToken);
        return Redirect(authorizationUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> OAuthCallback([FromQuery] string oauth_token, [FromQuery] string oauth_verifier)
    {
        var requestTokenSecret = await _cache.GetStringAsync($"RequestTokenSecret_{oauth_token}");
        if (string.IsNullOrEmpty(requestTokenSecret))
        {
            return BadRequest("Invalid request token secret");
        }

        var (accessToken, accessTokenSecret) = await _oauthService.GetAccessTokenAsync(oauth_token, requestTokenSecret, oauth_verifier);

        // Remove the temporary data from the cache
        await _cache.RemoveAsync($"RequestTokenSecret_{oauth_token}");

        // Store the accessToken and accessTokenSecret securely for future API calls
        // You might want to associate these with the user's account in your database

        return Ok($"OAuth flow completed successfully. Access Token: {accessToken}, Access Token Secret: {accessTokenSecret}");
    }
}