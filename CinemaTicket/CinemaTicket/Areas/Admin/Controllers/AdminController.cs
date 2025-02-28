using Microsoft.AspNetCore.Mvc;
using CinemaTicketAdmin.Data;
using System.Linq;


namespace CinemaTicketAdmin.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly CinemaTicketAdminDbContext _context;

        public AdminController(CinemaTicketAdminDbContext context)
        {
            _context = context;
        }

        public IActionResult AdminPage()
        {
            return View();
        }

    }
}
