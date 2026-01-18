using FilmArchive.Data;
using Microsoft.AspNetCore.Mvc;

public class MoviesController : Controller
{
    private readonly ApplicationDbContext _context;
    public MoviesController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(string searchTerm, string genre, string sortBy)
    {
        var movies = from m in _context.Movies select m;

        // Search Logic
        if (!string.IsNullOrEmpty(searchTerm))
            movies = movies.Where(s => s.Title.Contains(searchTerm) || s.Director.Contains(searchTerm));

        // Genre Filter
        if (!string.IsNullOrEmpty(genre) && genre != "All")
            movies = movies.Where(x => x.Genre == genre);

        // Sort Logic
        movies = sortBy switch
        {
            "rating" => movies.OrderByDescending(m => m.Rating),
            "alphabetical" => movies.OrderBy(m => m.Title),
            _ => movies.OrderByDescending(m => m.ReleaseYear),
        };

        return View(await movies.ToListAsync());
    }
    // Add Create, Edit, Delete Actions here...
}