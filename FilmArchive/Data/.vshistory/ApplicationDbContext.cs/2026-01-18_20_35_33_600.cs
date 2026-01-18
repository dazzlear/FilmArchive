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
            base.OnModelCreating(modelBuilder);

            // Explicitly set decimal precision to fix the truncation warning
            modelBuilder.Entity<Movie>()
                .Property(p => p.Rating)
                .HasPrecision(3, 1);
        }
    }
}