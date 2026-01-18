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

        // ADD THIS: Fixes the decimal truncation warning
        [Column(TypeName = "decimal(3, 1)")]
        public decimal Rating { get; set; } = 3.0m;

        public string? ImagePath { get; set; }
    }
}