using Crates.Backend.Models;

namespace Crates.Backend.Services
{
    public interface IDiscogsSearchService
    {
        Task<DiscogsSearchResult> SearchAsync(string query);
    }
}
