using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CinemaTicket.Data;
using CinemaTicket.Controllers;
using Microsoft.EntityFrameworkCore;
using Humanizer.Localisation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Threading.Channels;

namespace CinemaTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController : Controller
    {
        private readonly CinemaTicketDbContext _context;

        public MovieController(CinemaTicketDbContext context)
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
        public async Task<IActionResult> Details(int? id) // Thay đổi tham số thành int? id
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy thông tin phim theo movieID
            var movie = await _context.Movies
                .Include(m => m.Genres) // Lấy danh sách thể loại của phim
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            // Lấy danh sách tất cả thể loại để hiển thị trong dropdown
            var genres = await _context.MovieGenres
                .Select(g => g.GenreName)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            ViewBag.Genres = genres;

            return View(movie); // Truyền movie vào view
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

                // Gán thể loại đã chọn
                //if (selectedGenres != null)
                //{
                //    movie.Genres = await _context.MovieGenres
                //        .Where(g => selectedGenres.Contains(g.GenreId))
                //        .ToListAsync();
                //}

                var existingMovie = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.MovieId == movie.MovieId);
                movie.CreatedAt = DateTime.Now;
                movie.UpdatedAt = DateTime.Now;
                _context.Add(movie);
                await _context.SaveChangesAsync();
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

            ViewBag.Genres = await _context.MovieGenres.ToListAsync();
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

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
                    var existingMovie = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.MovieId == id);
                    if (existingMovie != null)
                    {
                        movie.CreatedAt = existingMovie.CreatedAt;
                        if (movie.ReleaseDate == default)
                        {
                            movie.ReleaseDate = existingMovie.ReleaseDate;
                        }
                    }

                    // Xử lý upload ảnh
                    if (file != null && file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName); // Lấy tên file gốc
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        if (!System.IO.File.Exists(filePath)) // Kiểm tra trùng lặp
                        {
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }
                            movie.Url = fileName;
                        }
                        else
                        {
                            //File đã tồn tại, dùng lại tên file.
                            movie.Url = fileName;
                        }
                    }
                    else
                    {
                        // Không có file mới, sử dụng ảnh cũ
                        if (existingMovie != null)
                        {
                            movie.Url = existingMovie.Url;
                        }
                    }

                    // Gán thể loại đã chọn (bỏ comment nếu cần)
                    // if (selectedGenres != null)
                    // {
                    //     movie.Genres = await _context.MovieGenres
                    //         .Where(g => selectedGenres.Contains(g.GenreId))
                    //         .ToListAsync();
                    // }

                    // Cập nhật UpdatedAt
                    movie.UpdatedAt = DateTime.Now;

                    // Cập nhật Movie
                    _context.Update(movie);
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

            ViewBag.Genres = await _context.MovieGenres.ToListAsync();
            return View(movie);
        }


        // GET: Movie/Delete/5
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

        // POST: Movie/Delete/5
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
