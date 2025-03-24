using Microsoft.AspNetCore.Mvc;
using System.Linq;
using CinemaTicket.ViewModels;
using System.Data.Entity;
using CinemaTicket.Data;


namespace CinemaTicket.Controllers
{
    public class AdminController : Controller
    {
        private readonly CinemaTicketDbContext _context;
        public AdminController(CinemaTicketDbContext context)
        {
            _context = context;
        }

        public IActionResult AdminPage()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);


            var genreData = _context.MovieGenreMappings
     .Include(gm => gm.MovieGenre) // Load thông tin thể loại
     .GroupBy(gm => gm.MovieGenre.GenreName) // Nhóm theo tên thể loại
     .Select(g => new
     {
         Genre = g.Key,       // Lấy tên thể loại
         Count = g.Count()    // Đếm số lượng phim thuộc thể loại đó
     })
     .ToList();

            var dashboardData = new DashboardViewModel
            {
                TotalReviews = _context.Reviews.Count(),
                RatingAverage = _context.Reviews.Any() ? _context.Reviews.Average(r => r.Rating) : 0,
                TicketPurchased = _context.Tickets.Count(),
                TotalMembers = _context.Customers.Count(),
                // Lây 7 phim có đánh giá trung bình cao nhất 
                hotMovies = _context.Movies
            .Include(m => m.Reviews)
            .Where(m => m.Reviews.Any()) // Lọc phim có đánh giá
            .Select(m => new MovieViewModel  
            {
                Title = m.Title,
                Url = m.Url,
                Description = m.Description,
               MovieRatingAverage = m.Reviews.Average(r => r.Rating) // tính trung bình 
            })
            .OrderByDescending(m => m.MovieRatingAverage) //Sắp xếp theo Rating cao nhất
            .Take(7)
            .ToList(),

                GenreLabels = genreData.Select(g => g.Genre).ToList(),
                GenreCounts = genreData.Select(g => g.Count).ToList()
            };
            return View(dashboardData);
        }

    }
}
