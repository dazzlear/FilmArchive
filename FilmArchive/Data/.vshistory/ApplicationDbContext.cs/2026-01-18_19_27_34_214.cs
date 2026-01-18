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
            modelBuilder.Entity<Movie>()
                .Property(p => p.Rating)
                .HasColumnType("decimal(3, 1)");
        }
    }
}