using Microsoft.AspNetCore.Mvc;
using CinemaTicketAdmin.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


namespace CinemaTicketAdmin.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController : Controller
    {
        private readonly CinemaTicketAdminDbContext _context;

        public MovieController(CinemaTicketAdminDbContext context)
        {
            _context = context;
        }

        // GET: MovieController
        public async Task<IActionResult> Index()
        {
            var movie = await _context.Movies
                 .ToListAsync();
            return View(movie);
        }

        // GET: Movie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // Lấy Movie dựa trên MovieId và load các Genres liên quan thông qua MovieGenreMappings
            var movie = await _context.Movies
                .Include(m => m.MovieGenreMappings)
                    .ThenInclude(mgm => mgm.Genre)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            return View(movie);
        }

        // GET: Movie/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Genres = await _context.MovieGenres.ToListAsync();
            return View();
        }

        // POST: Movie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile file, int[] selectedGenres)//[Bind("MovieID,Title,Duration,Language,ReleaseDate,Description,CreatedAt,UpdatedAt")] 
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    movie.Url = fileName;
                }
                else if (movie != null && string.IsNullOrEmpty(movie.Url))
                {
                    movie.Url = "AIsol.jpg"; // Đặt ảnh mặc định
                }// Nếu không có file mới và Url cũ trống

                movie.CreatedAt = DateTime.Now;
                movie.UpdatedAt = DateTime.Now;

                _context.Movies.Add(movie);
                await _context.SaveChangesAsync(); // Lưu movie trước để có MovieId

                // Thêm các MovieGenreMapping vào database
                if (selectedGenres != null)
                {
                    foreach (var genreId in selectedGenres)
                    {
                        var movieGenreMapping = new MovieGenreMapping
                        {
                            MovieId = movie.MovieId,
                            GenreId = genreId
                        };
                        _context.MovieGenreMappings.Add(movieGenreMapping);
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Genres = await _context.MovieGenres.ToListAsync();
            return View(movie);
        }

        // GET: Movie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var movie = await _context.Movies
                .Include(mpg => mpg.MovieGenreMappings)
                .ThenInclude(g => g.Genre)
                      .FirstAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            } //nếu movie không có
            ViewBag.Genres = await _context.MovieGenres.ToListAsync();
            return View(movie);
        }


        // POST: Movie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile file, int[] selectedGenres)
        {
            if (id != movie.MovieId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMovie = await _context.Movies
                .Include(m => m.MovieGenreMappings)
                .FirstOrDefaultAsync(m => m.MovieId == id);

                    if (existingMovie == null) // Kiểm tra xem existingMovie có tồn tại không
                    {
                        return NotFound(); // Trả về NotFound nếu không tìm thấy phim
                    }

                    // Xử lý upload ảnh
                    if (file != null && file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        existingMovie.Url = fileName;
                    }
                    else if (string.IsNullOrEmpty(existingMovie.Url))
                    {
                        existingMovie.Url = "movie-placeholder.jpg"; // Đặt ảnh mặc định
                    }

                    // Cập nhật các thuộc tính của movie
                    existingMovie.Title = movie.Title;
                    existingMovie.Duration = movie.Duration;
                    existingMovie.Language = movie.Language;
                    existingMovie.ReleaseDate = movie.ReleaseDate;
                    existingMovie.Description = movie.Description;
                    existingMovie.UpdatedAt = DateTime.Now;

                    // Xóa các MovieGenreMapping hiện tại
                    existingMovie.MovieGenreMappings.Clear();

                    // Thêm các MovieGenreMapping mới
                    if (selectedGenres != null)
                    {
                        foreach (var genreId in selectedGenres)
                        {
                            var movieGenreMapping = new MovieGenreMapping
                            {
                                MovieId = existingMovie.MovieId,
                                GenreId = genreId
                            };
                            existingMovie.MovieGenreMappings.Add(movieGenreMapping);
                        }
                    }
                    else
                    {
                        existingMovie.MovieGenreMappings.Clear();// = new List<MovieGenreMapping>();
                    }

                    _context.Movies.Update(existingMovie);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.MovieId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(movie);
        }

        // GET: Admin/Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Admin/Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.MovieId == id);
        }


    }
}
