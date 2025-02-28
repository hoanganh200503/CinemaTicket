using System.Diagnostics;
using CinemaTicket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaTicket.Data;
using System.Linq;

namespace CinemaTicket.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CinemaTicketDbContext _context;

        public HomeController(ILogger<HomeController> logger, CinemaTicketDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var movies = await _context.Movies
                    .Include(m => m.Genres)
                    .Where(m => m.ReleaseDate <= today)
                    .OrderByDescending(m => m.ReleaseDate)
                    .Take(8)  // Chỉ lấy 8 phim mới nhất
                    .ToListAsync();

                // Get unique genre names for the dropdown
                var genres = await _context.MovieGenres
                    .Select(g => g.GenreName)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToListAsync();

                ViewBag.Genres = genres;
                ViewBag.MovieType = "now-showing";
                return View(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách phim trang chủ");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách phim. Vui lòng thử lại.";
                return View(new List<Movie>());
            }
        }

        public async Task<IActionResult> NowShowing()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var movies = await _context.Movies
                    .Include(m => m.Genres)
                    .Where(m => m.ReleaseDate <= today)
                    .OrderByDescending(m => m.ReleaseDate)
                    .ToListAsync();
                return View(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách phim đang chiếu");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách phim. Vui lòng thử lại.";
                return View(new List<Movie>());
            }
        }

        public async Task<IActionResult> ComingSoon()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var movies = await _context.Movies
                    .Include(m => m.Genres)
                    .Where(m => m.ReleaseDate > today)
                    .OrderBy(m => m.ReleaseDate)
                    .ToListAsync();

                return View(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách phim sắp chiếu");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách phim. Vui lòng thử lại.";
                return View(new List<Movie>());
            }
        }

        public async Task<IActionResult> Search(string searchTerm, string genre)
        {
            try
            {
                var query = _context.Movies.Include(m => m.Genres).AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(m => m.Title.Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(genre))
                {
                    query = query.Where(m => m.Genres.Any(g => g.GenreName == genre));
                }

                var movies = await query.ToListAsync();
                ViewBag.SearchTerm = searchTerm;
                ViewBag.SelectedGenre = genre;

                // Get unique genre names for the dropdown
                var genres = await _context.MovieGenres
                    .Select(g => g.GenreName)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToListAsync();

                ViewBag.Genres = genres;

                return View(movies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi khi tìm kiếm phim");
                TempData["Error"] = "Có lỗi xảy ra khi tìm kiếm phim. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
