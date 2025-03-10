using Microsoft.AspNetCore.Mvc;
using CinemaTicket.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Controllers     //Đổi lại namespace (cũ  CinemaTicketApp.Controllers)
{
    public class AccountController : Controller
    {
        private readonly CinemaTicketDbContext _context; //Đổi lại Dataconect (cũ CinemaTicketAppDvContext) => CinemaTicketDbContext

        public AccountController(CinemaTicketDbContext context) //Đổi lại Dataconect (cũ CinemaTicketAppDvContext) => CinemaTicketDbContext
        {
            _context = context;
        }



        [HttpGet]
        public IActionResult Login()
        {
            return RedirectToAction("DangNhap", "Customer");
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Lưu email vào phiên
            HttpContext.Session.SetString("UserEmail", email);
            return RedirectToAction("DangNhap", "Customer");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null)
            {
                return RedirectToAction("DangNhap", "Customer");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.Email == userEmail);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpGet]
        public IActionResult Details()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null)
            {
                return RedirectToAction("DangNhap", "Customer");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.Email == userEmail);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpGet]
        public IActionResult Edit(bool changePassword = false)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null)
            {
                return RedirectToAction("DangNhap", "Customer");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.Email == userEmail);
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
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null)
            {
                return RedirectToAction("DangNhap", "Customer");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.Email == userEmail);
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
