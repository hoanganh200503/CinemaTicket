using Microsoft.AspNetCore.Mvc;
using System.Linq;
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
            return View("AdminPage");
        }

    }
}
