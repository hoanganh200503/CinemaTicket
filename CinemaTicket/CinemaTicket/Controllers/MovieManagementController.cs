using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CinemaTicket.Data;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaTicket.Controllers
{
    public class MovieManagementController : Controller
    {
        private readonly CinemaTicketDbContext _context;

        public MovieManagementController(CinemaTicketDbContext context)
        {
            _context = context;
        }

        // GET: MoviemanagementController
        public IActionResult Index(  string? txtSearch, int? genreId,int page = 1)
        {   
                     page = page < 1 ? 1 : page;
            int pageSize = 6;

            var movies = _context.Movies.AsQueryable(); // Sử dụng AsQueryable để trì hoãn việc lấy dữ liệu

            if (!string.IsNullOrEmpty(txtSearch))
            {
                movies = movies.Where(m => m.Title.ToLower().Contains(txtSearch.ToLower())); // Tìm kiếm không phân biệt chữ hoa chữ thường
            }

            var pagedMovies = movies.ToPagedList(page, pageSize); // Phân trang kết quả

// Truyền danh sách thể loại vào ViewData để hiển thị dropdown list
            var genres = _context.MovieGenres.ToList();
            genres.Insert(0, new MovieGenre { GenreId = 0, GenreName = "--Choose Genre--" });
            ViewData["GenreId"] = new SelectList(genres, "GenreId", "GenreName", genreId);

            return View(pagedMovies);
        }

        public IActionResult FilterByGenre(int? genreId, int page = 1)
        {
            genreId = genreId ?? 0;
            var genres = _context.MovieGenres.ToList();
            //GenreId GenreName
            genres.Insert(0, new MovieGenre { GenreId = 0, GenreName = "--Choose Genre--" });
            ViewData["GenreId"] = new SelectList(genres, "GenreId", "GenreName", genreId);
            List<Movie> movies; // Khai báo movies là List<Movie>
            if (genreId == 0) // Nếu chọn "--Choose Genre--", lấy tất cả phim
            {
                movies = _context.Movies
                    .Include(mg => mg.MovieGenreMappings)
                    .ThenInclude(mg => mg.MovieGenre)
                    .ToList();
            }
            else // Lấy phim theo genreId
            {
                movies = _context.Movies
                    .Include(mg => mg.MovieGenreMappings)
                    .ThenInclude(mg => mg.MovieGenre)
                    .Where(m => m.MovieGenreMappings.Any(mgm => mgm.GenreId == genreId))
                    .ToList();
            }
            int pageSize = 6; // Đặt số lượng phim trên một trang
            var pagedMovies = movies.ToPagedList(page, pageSize); // Chuyển đổi List<Movie> thành IPagedList<Movie>
            return View("Index", pagedMovies);
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
                    .ThenInclude(mgm => mgm.MovieGenre)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            return View(movie);
        }

        // GET: Movie/Create
        public async Task<IActionResult> Create()
        {
            var movie = new Movie(); // Tạo một đối tượng Movie mới
                                     // Lấy danh sách tất cả các thể loại phim
            var genres = await _context.MovieGenres
                .Distinct()
                .ToListAsync();

            // Gán danh sách thể loại phim vào ViewBag
            ViewBag.Genres = genres;

            return View(movie); // Truyền đối tượng Movie mới vào view
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
                var existingMovie = await _context.Movies.FirstOrDefaultAsync(m => m.Title == movie.Title);
                if (existingMovie != null)
                {
                    ModelState.AddModelError("Title", "Movie is exixted!");
                    ViewBag.Genres = await _context.MovieGenres.ToListAsync();
                    return View(movie);
                }
                if (movie.Duration <= 0)
                {
                    ModelState.AddModelError("Duration", "The duration appcept positive only!");
                    ViewBag.Genres = await _context.MovieGenres.ToListAsync();
                    return View(movie);
                }

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
                      .FirstOrDefaultAsync(m => m.MovieId == id);

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
