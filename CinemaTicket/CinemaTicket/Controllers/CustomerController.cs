using Microsoft.AspNetCore.Mvc;
using CinemaTicketApp.Data;
using CinemaTicketApp.Models;
using System.Linq;

namespace CinemaTicketApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var customers = _context.Customers.ToList();
            return View(customers);
        }
    }
}
