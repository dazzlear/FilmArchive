using System.ComponentModel.DataAnnotations;

namespace FilmArchive.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Director { get; set; }
        public string Genre { get; set; }
        public int ReleaseYear { get; set; }
        [Range(1, 5)]
        public decimal Rating { get; set; }
        public string? ImagePath { get; set; }
    }
}