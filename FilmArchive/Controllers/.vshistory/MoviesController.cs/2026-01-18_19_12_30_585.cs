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
        public async Task<IActionResult> Index(string searchTerm, string genre, string sortBy)
        {
            var movies = from m in _context.Movies select m;

            // Search
            if (!string.IsNullOrEmpty(searchTerm))
                movies = movies.Where(s => s.Title.Contains(searchTerm) || s.Director.Contains(searchTerm));

            // Filter
            if (!string.IsNullOrEmpty(genre) && genre != "All")
                movies = movies.Where(x => x.Genre == genre);

            // Sort
            movies = sortBy switch
            {
                "rating" => movies.OrderByDescending(m => m.Rating),
                "alphabetical" => movies.OrderBy(m => m.Title),
                _ => movies.OrderByDescending(m => m.ReleaseYear),
            };

            return View(await movies.ToListAsync());
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
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
            string extension = Path.GetExtension(imageFile.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmddssfff") + extension;
            string path = Path.Combine(wwwRootPath + "/uploads/", fileName);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return "/uploads/" + fileName;
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null) _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}