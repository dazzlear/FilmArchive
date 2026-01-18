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
            var moviesQuery = _context.Movies.AsQueryable();

            // New Category Filter (All, Movie, Series)
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                // Assuming your Model has a 'Category' property
                moviesQuery = moviesQuery.Where(x => x.Category == category);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                moviesQuery = moviesQuery.Where(s => s.Title.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(genre) && genre != "Genre")
            {
                moviesQuery = moviesQuery.Where(x => x.Genre == genre);
            }

            return View(await moviesQuery.ToListAsync());
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

            // If we got this far, something failed. 
            // Return to Index view with the list of movies to keep the Modal/Single-page feel.
            TempData["Error"] = "Failed to archive film. Please check your inputs.";
            var movies = await _context.Movies.ToListAsync();
            return View("Index", movies);
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
                        // Preserve the existing image path if no new file is uploaded
                        var existingMovie = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                        movie.ImagePath = existingMovie?.ImagePath;
                    }

                    _context.Update(movie);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Archive entry updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Could not update entry. Validation failed.";
            var movies = await _context.Movies.ToListAsync();
            return View("Index", movies);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                // Optional: Delete physical image file from server if needed
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

            // Generate unique filename to prevent overwriting
            fileName = fileName + "_" + DateTime.Now.ToString("yymmddHHmmssfff") + extension;

            // Path.Combine is safer than string concatenation
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