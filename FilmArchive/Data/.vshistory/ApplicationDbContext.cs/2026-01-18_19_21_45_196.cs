using Microsoft.EntityFrameworkCore;
using FilmArchive.Models;

namespace FilmArchive.Data
{
    // Make sure this name matches what you use in Program.cs and your Controller
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
    }
}