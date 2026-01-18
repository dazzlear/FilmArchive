using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmArchive.Data;
using FilmArchive.Models;

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


        public async Task<IActionResult> Index(string searchTerm, string genre, string category, string sortBy)
        {
            ViewBag.Genres = Movie.GetGenres();
            var moviesQuery = _context.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(category) && category != "All")
                moviesQuery = moviesQuery.Where(x => x.Category == category);

            if (!string.IsNullOrEmpty(searchTerm))
                moviesQuery = moviesQuery.Where(m => m.Title.Contains(searchTerm) || m.Director.Contains(searchTerm));

            if (!string.IsNullOrEmpty(genre) && genre != "All")
                moviesQuery = moviesQuery.Where(x => x.Genre == genre);

            moviesQuery = sortBy switch
            {
                "rating" => moviesQuery.OrderByDescending(m => m.Rating),
                "alphabetical" => moviesQuery.OrderBy(m => m.Title),
                // CHANGE THIS LINE: Sort by Id descending to show newest first
                _ => moviesQuery.OrderByDescending(m => m.Id),
            };

            return View(await moviesQuery.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile? imageFile)
        {
            
            ModelState.Remove("imageFile");
            ModelState.Remove("ImagePath");

            if (imageFile != null)
            {
                var permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!permittedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Only JPG and PNG allowed.";
                    return RedirectToAction(nameof(Index));
                }

                movie.ImagePath = await SaveImage(imageFile);
            }

            if (ModelState.IsValid)
            {
                movie.Rating = Math.Clamp(movie.Rating, 1.0, 5.0);
                _context.Add(movie);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Film successfully archived.";
                return RedirectToAction(nameof(Index));
            }

            var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            TempData["Error"] = $"Create Failed: {error ?? "Check required fields."}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile? imageFile)
        {
            if (id != movie.Id) return NotFound();

            ModelState.Remove("imageFile");
            ModelState.Remove("ImagePath");

            if (imageFile != null)
            {
                var permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!permittedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Only JPG and PNG allowed.";
                    return RedirectToAction(nameof(Index));
                }
                movie.ImagePath = await SaveImage(imageFile);
            }
            else
            {
                var existing = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                movie.ImagePath = existing?.ImagePath;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    movie.Rating = Math.Clamp(movie.Rating, 1.0, 5.0);
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Update Successful";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Update Failed: Invalid data";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Entry removed";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            // ensure the directory exists
            string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string path = Path.Combine(uploadDir, fileName);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return "/uploads/" + fileName;
        }

        private bool MovieExists(int id) => _context.Movies.Any(e => e.Id == id);
    }
}