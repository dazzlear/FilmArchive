using System;
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

        [Required]
        public string Genre { get; set; }

        public int ReleaseYear { get; set; }

        [Range(0, 10)]
        public double Rating { get; set; }

        public string ImageUrl { get; set; }  
        public string ImageUrl { get; set; }  
    }
}
