using Microsoft.AspNetCore.Mvc;
using CinemaTicketApp.Data;
using CinemaTicketApp.Models;
using System.Linq;

namespace CinemaTicketApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AdminPage()
        {
            return View();
        }
    
}
}
