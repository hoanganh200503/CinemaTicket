using Microsoft.AspNetCore.Mvc;
using CinemaTicket.Data;
using System.Linq;


namespace CinemaTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly CinemaTicketDbContext _context;

        public AdminController(CinemaTicketDbContext context)
        {
            _context = context;
        }

        public IActionResult AdminPage()
        {
            return View();
        }

    }
}
