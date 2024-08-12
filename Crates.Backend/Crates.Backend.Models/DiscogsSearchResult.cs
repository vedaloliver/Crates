
using System;

namespace Crates.Backend.Models
{
    // The object to pass back up to the frontedn
    public class DiscogsSearchResult
    {
        public string? Title { get; set; }

        public string? Year { get; set; }

        public float Price { get; set; }

        public string ImageUrl { get; set; }
       

    }
}
