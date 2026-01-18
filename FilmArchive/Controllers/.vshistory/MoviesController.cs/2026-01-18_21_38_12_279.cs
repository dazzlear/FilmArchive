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

        public async Task<IActionResult> Index(string searchTerm, string genre, string category)
        {
            ViewBag.Genres = Movie.GetGenres(); // Fixes NullReference at line 80
            var movies = _context.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm)) movies = movies.Where(s => s.Title.Contains(searchTerm));
            if (!string.IsNullOrEmpty(genre) && genre != "All") movies = movies.Where(x => x.Genre == genre);
            if (!string.IsNullOrEmpty(category) && category != "All") movies = movies.Where(x => x.Category == category);

            return View(await movies.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null) movie.ImagePath = await SaveImage(imageFile);
                _context.Add(movie);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Film successfully archived.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Genres = Movie.GetGenres();
            return View("Index", await _context.Movies.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile? imageFile)
        {
            if (id != movie.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null) movie.ImagePath = await SaveImage(imageFile);
                    else
                    {
                        var existing = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                        movie.ImagePath = existing?.ImagePath;
                    }
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Update Successful.";
                }
                catch (DbUpdateConcurrencyException) { if (!MovieExists(movie.Id)) return NotFound(); throw; }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Genres = Movie.GetGenres();
            return View("Index", await _context.Movies.ToListAsync());
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