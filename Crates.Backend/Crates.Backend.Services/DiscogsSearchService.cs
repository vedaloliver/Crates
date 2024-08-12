using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Crates.Backend.Models;
using Crates.Backend.Services.Helpers;
using Crates.Backend.Services;

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
        // Search the Discogs API to retrieve release data
        var baseUrl = "https://api.discogs.com/database/search";
        var requestUrl = $"{baseUrl}?q={Uri.EscapeDataString(query)}&key={Uri.EscapeDataString(consumerKey)}&secret={Uri.EscapeDataString(consumerSecret)}";

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        requestMessage.Headers.Add("User-Agent", "DiscogsSearchApp/1.0");

        var response = await this.httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        string jsonResponse = await response.Content.ReadAsStringAsync();
        SearchResult searchResult = await JsonDeserialiser.DeserializeSearchQuery(jsonResponse);

        ListingResult listingResult = null;
        // Use the release ID from the search result to get the feed data
        if (searchResult != null && searchResult.Results.Count > 0)
        {
            string releaseId = searchResult.Results[0].Id.ToString();
            listingResult = await this.GetFeedDataAsync(releaseId);
        }

        //  a data object to return back to the frontend 
        if (listingResult != null)
        {
            return await ConstructRecordObject(searchResult, listingResult);

        }

        return null;
    }

    private async Task<ListingResult> GetFeedDataAsync(string releaseId)
    {
        var feedUrl = $"https://www.discogs.com/sell/mplistrss?output=rss&release_id={Uri.EscapeDataString(releaseId)}";

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, feedUrl);
        requestMessage.Headers.Add("User-Agent", "DiscogsSearchApp/1.0");
        requestMessage.Headers.Add("Accept", "application/rss+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        requestMessage.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        requestMessage.Headers.Add("Referer", "https://www.discogs.com");

        var response = await this.httpClient.SendAsync(requestMessage);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new Exception("Access to the Discogs RSS feed is forbidden. Please check your request headers or API access.");
        }

        response.EnsureSuccessStatusCode();

        var xmlContent = await response.Content.ReadAsStringAsync();
        return ParseFeedToJson(xmlContent);
    }

    private ListingResult ParseFeedToJson(string xmlContent)
    {
        var xdoc = XDocument.Parse(xmlContent);
        var jsonResult = new ListingResult
        {
            Title = xdoc.Root.Element("{http://www.w3.org/2005/Atom}title")?.Value,
            Updated = xdoc.Root.Element("{http://www.w3.org/2005/Atom}updated")?.Value,
            Entries = new List<ListingEntry>()
        };

        foreach (var entry in xdoc.Root.Elements("{http://www.w3.org/2005/Atom}entry"))
        {
            var feedEntry = new ListingEntry
            {
                Id = entry.Element("{http://www.w3.org/2005/Atom}id")?.Value,
                Title = entry.Element("{http://www.w3.org/2005/Atom}title")?.Value,
                Updated = entry.Element("{http://www.w3.org/2005/Atom}updated")?.Value,
                Link = entry.Element("{http://www.w3.org/2005/Atom}link")?.Attribute("href")?.Value,
                Summary = entry.Element("{http://www.w3.org/2005/Atom}summary")?.Value
            };

            jsonResult.Entries.Add(feedEntry);
        }

        return jsonResult;
    }

    private async Task<DiscogsSearchResult> ConstructRecordObject(SearchResult searchResult, ListingResult listing)
    {
        string parsedTitle = this.ParseTitle(listing.Title);

        float priceValue = this.GetPriceValue(listing.Entries);

        DiscogsSearchResult entry = new DiscogsSearchResult
        {
            Title = parsedTitle,
            Year = searchResult.Results[0].Year,
            Price = priceValue,
            ImageUrl = searchResult.Results[0].Thumb,
        };

        return entry;

    }
    private string ParseTitle(string title)
    {
        // Define the suffixes you want to remove
        string[] suffixesToRemove = new string[]
        {
        "(Vinyl) For Sale at Discogs Marketplace",
        "(Vinyl) For Sale",
        "(Vinyl)"
        };

        // Loop through each suffix and remove it if found
        foreach (string suffix in suffixesToRemove)
        {
            int index = title.IndexOf(suffix);
            if (index >= 0)
            {
                // Remove the suffix and trim any extra spaces
                return title.Substring(0, index).Trim();
            }
        }

        // If no suffix is found, return the title as is
        return title;
    }

    private float GetPriceValue(List<ListingEntry> listingEntries)
    {
        List<float> priceValues = new List<float>();

        foreach (var entry in listingEntries)
        {
            // Extract the price and convert to GBP
            float priceInGbp = ExtractPriceFromSummary(entry.Summary);
            if (priceInGbp >= 0)
            {
                priceValues.Add(priceInGbp);
            }
        }

        return priceValues.Count > 0 ? priceValues.Average() : 0;
    }

    private float ExtractPriceFromSummary(string summary)
    {
        // Split the summary string by spaces to get the currency and price
        string[] parts = summary.Split(' ');

        if (parts.Length > 1 && float.TryParse(parts[1], out float price))
        {
            // Get the currency code
            string currency = parts[0];

            // Convert the price to GBP
            float priceInGbp = ConvertToGbp(price, currency);
            return priceInGbp;
        }

        return -1; // Return a negative value if parsing fails
    }

    private float ConvertToGbp(float price, string currency)
    {
        // Define the conversion rates to GBP (these are example rates)
        var exchangeRates = new Dictionary<string, float>
    {
        { "SD", 0.005f },   // Example conversion rate
        { "GBP", 1.0f },    // GBP to GBP is 1:1
        { "EUR", 0.85f },   // Example conversion rate
        { "CAD", 0.59f },   // Example conversion rate
        { "AUD", 0.55f },   
        { "JPY", 0.0068f }, 
        { "CHF", 0.78f },   // Example conversion rate
        { "MXN", 0.042f },  // Example conversion rate
        { "BRL", 0.16f },   // Example conversion rate
        { "NZD", 0.51f },   // Example conversion rate
        { "SEK", 0.072f },  // Example conversion rate
        { "ZAR", 0.051f }   // Example conversion rate
    };

        // Lookup the exchange rate for the given currency and convert to GBP
        if (exchangeRates.TryGetValue(currency, out float rate))
        {
            return price * rate;
        }

        // If currency is not recognized, return -1 (or handle as needed)
        return -1;
    }
}
