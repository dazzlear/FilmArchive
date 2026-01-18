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
        public string Category { get; set; } = "Movie";

        public int ReleaseYear { get; set; } 

        // UPDATED: Range is now 1.0 to 5.0
        [Column(TypeName = "decimal(3, 1)")]
        [Range(1.0, 5.0, ErrorMessage = "Rating must be between 1 and 5.")]
        public double Rating { get; set; } = 3.0;

        public string? ImagePath { get; set; }

        public static List<string> GetGenres() =>
            new() { "Action", "Sci-Fi", "Crime", "Drama", "Comedy", "Horror", "Thriller", "Adventure" };
    }
}