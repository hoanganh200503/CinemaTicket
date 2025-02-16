using Microsoft.AspNetCore.Mvc;
using CinemaTicketApp.Models;
using CinemaTicketApp.Data;

namespace CinemaTicketApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == email && c.Password == password);

            if (customer == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View();
            }

            // Lưu thông tin người dùng vào Session
            HttpContext.Session.SetInt32("CustomerId", customer.CustomerID);
            HttpContext.Session.SetString("CustomerEmail", customer.Email);
            HttpContext.Session.SetString("CustomerName", customer.Name);
            HttpContext.Session.SetString("CustomerRole", customer.Role);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerId);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpGet]
        public IActionResult Details()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerId);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpGet]
        public IActionResult Edit(bool changePassword = false)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerId);
            if (customer == null)
            {
                return NotFound();
            }

            ViewData["OpenPasswordChange"] = changePassword;
            return View(customer);
        }

        [HttpPost]
        public IActionResult Edit(string name, string email, string currentPassword, string newPassword)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerId);
            if (customer == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin cơ bản
            customer.Name = name;
            customer.Email = email;

            // Cập nhật mật khẩu nếu có
            if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword))
            {
                if (customer.Password != currentPassword)
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không đúng");
                    return View(customer);
                }
                customer.Password = newPassword;
            }

            customer.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            // Cập nhật session
            HttpContext.Session.SetString("CustomerName", customer.Name);
            HttpContext.Session.SetString("CustomerEmail", customer.Email);

            return RedirectToAction("Profile");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
