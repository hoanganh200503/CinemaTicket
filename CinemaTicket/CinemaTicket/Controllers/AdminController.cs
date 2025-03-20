using Microsoft.AspNetCore.Mvc;
using System.Linq;
using CinemaTicket.Models;
using CinemaTicket.ViewModels;
using System.Data.Entity;


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
            var dashboardData = new DashboardViewModel
            {
                TotalReviews = _context.Reviews.Count(),
                RatingAverage = _context.Reviews.Any() ? _context.Reviews.Average(r => r.Rating) : 0,
                TicketPurchased = _context.Tickets.Count(),
                TotalMembers = _context.Customers.Count(),

                hotMovies = _context.Movies
                    .Include(m => m.MovieGenreMappings)
                    .Where(m => m.ReleaseDate <= today)
                    .OrderByDescending(m => m.ReleaseDate)
                    .Take(8)  // Chỉ lấy 8 phim mới nhất
                    .ToList()

            };
                   



            return View(dashboardData);
        }

    }
}
