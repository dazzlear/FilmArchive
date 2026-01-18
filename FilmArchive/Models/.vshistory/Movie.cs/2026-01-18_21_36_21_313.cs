using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmArchive.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Director { get; set; } = string.Empty;
        public string Genre { get; set; } = "Action";

        [Required]
        public string Category { get; set; } = "Movie"; // Fixes CS1061

        public int ReleaseYear { get; set; } = 2026;

        [Column(TypeName = "decimal(3, 1)")] // Fixes precision mismatch
        [Range(0.0, 10.0)]
        public double Rating { get; set; } = 3.0;

        public string? ImagePath { get; set; }

        // Helper to ensure ViewBag.Genres is never null
        public static List<string> GetGenres() =>
            new() { "Action", "Sci-Fi", "Crime", "Drama", "Comedy", "Horror", "Thriller", "Adventure" };
    }
}