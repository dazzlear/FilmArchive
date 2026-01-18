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

        // GET: Movies
        public async Task<IActionResult> Index(string searchTerm, string genre, string category, string sortBy)
        {
            // Populate Genres for the dropdowns to prevent NullReference errors
            ViewBag.Genres = Movie.GetGenres();

            var moviesQuery = _context.Movies.AsQueryable();

            // 1. Category Filter Logic
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                moviesQuery = moviesQuery.Where(x => x.Category == category);
            }

            // 2. Search Logic
            if (!string.IsNullOrEmpty(searchTerm))
            {
                moviesQuery = moviesQuery.Where(m => m.Title.Contains(searchTerm) || m.Director.Contains(searchTerm));
            }

            // 3. Genre Filter Logic
            if (!string.IsNullOrEmpty(genre) && genre != "All")
            {
                moviesQuery = moviesQuery.Where(x => x.Genre == genre);
            }

            // 4. Sorting Logic
            moviesQuery = sortBy switch
            {
                "rating" => moviesQuery.OrderByDescending(m => m.Rating),
                "alphabetical" => moviesQuery.OrderBy(m => m.Title),
                _ => moviesQuery.OrderByDescending(m => m.ReleaseYear),
            };

            return View(await moviesQuery.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile? imageFile)
        {
            // 1. Image Extension Validation
            if (imageFile != null)
            {
                var permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Upload failed: Only JPG and PNG files are allowed.";
                    return RedirectToAction(nameof(Index));
                }

                movie.ImagePath = await SaveImage(imageFile);
            }

            // 2. Model State & Logic Validation
            if (ModelState.IsValid)
            {
                // Safety Check: Clamp rating 1-5
                movie.Rating = Math.Clamp(movie.Rating, 1.0, 5.0);

                _context.Add(movie);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Film successfully archived.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Entry Failed: Please check your input values.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile? imageFile)
        {
            if (id != movie.Id) return NotFound();

            // 1. Image Extension Validation
            if (imageFile != null)
            {
                var permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!permittedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Update failed: Only JPG and PNG allowed.";
                    return RedirectToAction(nameof(Index));
                }
                movie.ImagePath = await SaveImage(imageFile);
            }
            else
            {
                // Keep the existing image if no new file is uploaded
                var existing = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                movie.ImagePath = existing?.ImagePath;
            }

            // 2. Save Changes
            if (ModelState.IsValid)
            {
                try
                {
                    movie.Rating = Math.Clamp(movie.Rating, 1.0, 5.0);
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Update Successful.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Update Failed: Please check your data.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null) { _context.Movies.Remove(movie); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(imageFile.FileName) + "_" + DateTime.Now.ToString("yymmddHHmmss") + Path.GetExtension(imageFile.FileName);
            string path = Path.Combine(_hostEnvironment.WebRootPath, "uploads", fileName);
            using (var fileStream = new FileStream(path, FileMode.Create)) { await imageFile.CopyToAsync(fileStream); }
            return "/uploads/" + fileName;
        }

        private bool MovieExists(int id) => _context.Movies.Any(e => e.Id == id);
    }
}