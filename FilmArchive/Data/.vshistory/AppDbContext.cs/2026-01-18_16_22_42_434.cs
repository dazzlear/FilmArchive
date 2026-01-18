using Microsoft.EntityFrameworkCore;

namespace FilmArchive.Models
{
    public class FilmArchiveContext : AppDbContext
    {
        public FilmArchiveContext(DbContextOptions<FilmArchiveContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
    }
}
