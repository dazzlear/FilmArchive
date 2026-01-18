using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmArchive.Data;
using FilmArchive.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FilmArchive.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public MoviesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string searchTerm, string genre, string category, string sortBy)
        {
            var moviesQuery = _context.Movies.AsQueryable();

            // Category Filter: "All", "Movie", "Series"
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                moviesQuery = moviesQuery.Where(x => x.Category == category);
            }

            // Search by Title or Director
            if (!string.IsNullOrEmpty(searchTerm))
            {
                moviesQuery = moviesQuery.Where(m => m.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || m.Director.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Genre Filter: "All" or specific genre
            if (!string.IsNullOrEmpty(genre) && genre != "All")
            {
                moviesQuery = moviesQuery.Where(x => x.Genre == genre);
            }

            // Sorting
            switch (sortBy)
            {
                case "rating":
                    moviesQuery = moviesQuery.OrderByDescending(m => m.Rating);
                    break;
                case "alphabetical":
                    moviesQuery = moviesQuery.OrderBy(m => m.Title);
                    break;
                case "newest":
                    moviesQuery = moviesQuery.OrderByDescending(m => m.ReleaseYear);
                    break;
                default:
                    moviesQuery = moviesQuery.OrderByDescending(m => m.ReleaseYear);
                    break;
            }

            var movies = await moviesQuery.ToListAsync();
            return View(movies);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    movie.ImagePath = await SaveImage(imageFile);
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Film successfully archived.";
                return RedirectToAction(nameof(Index));
            }

            if (movie.Rating < 0 || movie.Rating > 10)
            {
                movie.Rating = 5.0; // Set default rating if invalid
            }

            TempData["Error"] = "Failed to archive film. Please check your inputs.";
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile? imageFile)
        {
            if (id != movie.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        movie.ImagePath = await SaveImage(imageFile);
                    }
                    else
                    {
                        // Keep the original image if none is uploaded
                        var existing = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                        movie.ImagePath = existing?.ImagePath;
                    }

                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Archive entry updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Movies.Any(e => e.Id == movie.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                // Optionally delete the physical image if needed
                if (!string.IsNullOrEmpty(movie.ImagePath))
                {
                    var imagePath = Path.Combine(_hostEnvironment.WebRootPath, movie.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Film removed from archive.";
            }
            else
            {
                TempData["Error"] = "Could not find the film to delete.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
            string extension = Path.GetExtension(imageFile.FileName);

            // Generate unique file name to avoid overwriting
            fileName = fileName + "_" + DateTime.Now.ToString("yymmddHHmmssfff") + extension;
            string path = Path.Combine(wwwRootPath, "uploads", fileName);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/uploads/" + fileName;
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}