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

        // ADD THIS: Fixes CS1061 error
        [Required]
        public string Category { get; set; } = "Movie"; // Movie or Series

        public int ReleaseYear { get; set; } = DateTime.Now.Year;

        [Range(0, 10)]
        public double Rating { get; set; }

        public string? ImagePath { get; set; }

        public static List<string> GetGenres() {
            return new List<string> {
        "Action", "Sci-Fi", "Drama", "Comedy", "Horror", "Thriller", "Adventure", "Romance", "Fantasy", "Animation", "Documentary"  };
        }


    }
}