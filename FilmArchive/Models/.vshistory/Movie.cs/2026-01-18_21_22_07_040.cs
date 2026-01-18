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

        // Expanded Genre List logic
        public string Genre { get; set; } = "Action";

        [Required]
        public string Category { get; set; } = "Movie"; // "Movie" or "Series"

        [Display(Name = "Release Year")]
        public int ReleaseYear { get; set; } = 2026;

        // FIX: Force SQL Server to use decimal(3,1) to keep the decimal point
        [Column(TypeName = "decimal(3, 1)")]
        [Range(0.0, 10.0)]
        public double Rating { get; set; } = 3.0;

        public string? ImagePath { get; set; }

        // Helper for the View
        public static List<string> GetGenres()
        {
            return new List<string> {
                "Action", "Sci-Fi", "Crime", "Drama",
                "Comedy", "Horror", "Thriller", "Adventure", "Animation"
            };
        }
    }
}