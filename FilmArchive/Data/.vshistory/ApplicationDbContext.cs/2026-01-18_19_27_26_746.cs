using Microsoft.EntityFrameworkCore;
using FilmArchive.Models;

namespace FilmArchive.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Movie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // This fix prevents the "precision" warning seen in your 3rd image
            modelBuilder.Entity<Movie>()
                .Property(p => p.Rating)
                .HasColumnType("decimal(3, 1)");
        }
    }
}