using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilmArchive.Models;
using System.Linq;
using System.Threading.Tasks;

namespace FilmArchive.Controllers
{
    public class MoviesController : Controller
    {
        private readonly FilmArchiveContext _context;

        public MoviesController(FilmArchiveContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.ToListAsync();
            return View(movies);  // Pass movies to the view
        }

        // Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Director,Genre,ReleaseYear,Rating")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));  // Redirect to the Index action
            }
            return View(movie);  // Return the view if the model state is invalid
        }

        // Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);  // Pass the movie to the Edit view
        }

        // Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Director,Genre,ReleaseYear,Rating")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));  // Redirect to the movie list after edit
            }

            return View(movie);  // Return the view if the model state is invalid
        }

        //Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);  // Pass the movie to the Delete view
        }

        // Movies/Delete/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));  // Redirect to the movie list after deletion
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);  // Check if a movie with the given id exists
        }
    }
}

