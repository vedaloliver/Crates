using System.Collections.Generic;

namespace Crates.Backend.Models
{
    public class SearchResult
    {
        public Pagination? Pagination { get; set; }
        public List<Release>? Results { get; set; }
    }

    public class Pagination
    {
        public int? PerPage { get; set; }
        public int? Pages { get; set; }
        public int? Page { get; set; }
        public Urls? Urls { get; set; }
        public int? Items { get; set; }
    }

    public class Urls
    {
        public string? Last { get; set; }
        public string? Next { get; set; }
    }

    public class Release
    {
        public List<string>? Style { get; set; }
        public string? Thumb { get; set; }
        public string? Title { get; set; }
        public string? Country { get; set; }
        public List<string>? Format { get; set; }
        public string? Uri { get; set; }
        public Community? Community { get; set; }
        public List<string>? Label { get; set; }
        public string? Catno { get; set; }
        public string? Year { get; set; }
        public List<string>? Genre { get; set; }
        public string? ResourceUrl { get; set; }
        public string? Type { get; set; }
        public int? Id { get; set; }
        public List<string>? Barcode { get; set; }
    }

    public class Community
    {
        public int Want { get; set; }
        public int Have { get; set; }
    }

}
