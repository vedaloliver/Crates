namespace Crates.Backend.Services
{
    public interface IDiscogsSearchService
    {
        Task<string> SearchAsync(string query);
    }
}
