

namespace Crates.Backend.Models
{
    public class ListingResult
    {
        public string Title { get; set; }
        public string Updated { get; set; }
        public List<ListingEntry> Entries { get; set; }
    }
}
