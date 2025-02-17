using Microsoft.AspNetCore.Mvc;
using CinemaTicketApp.Data;
using CinemaTicketApp.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CinemaTicketApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Hàm băm MD5
        public string HashPassword(string password)
        {
            using (MD5 md5 = MD5.Create()) // Sử dụng MD5 để băm mật khẩu
            {
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Hiển thị trang form đăng ký
        public IActionResult Dangky()
        {
            return View();
        }

        // Xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Dangky(Customer customer, string ConfirmPassword)
        {
            if (customer.Password != ConfirmPassword)
            {
                ModelState.AddModelError("Password", "Mật khẩu xác nhận không khớp.");
                return View(customer);
            }

            customer.CreatedAt = DateTime.Now;
            customer.UpdatedAt = DateTime.Now;
            customer.Password = HashPassword(customer.Password);

            _context.Customers.Add(customer);
            _context.SaveChanges();

            return RedirectToAction("DangNhap"); // Chuyển sang trang đăng nhập sau khi đăng ký
        }

        // Hiển thị form đăng nhập
        public IActionResult DangNhap()
        {
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangNhap(string email, string password)
        {
            string hashedPassword = HashPassword(password);

            var customer = _context.Customers
                .FirstOrDefault(c => c.Email == email && c.Password == hashedPassword);

            if (customer == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            // Lưu thông tin người dùng vào session 
            HttpContext.Session.SetString("UserEmail", customer.Email);
            HttpContext.Session.SetString("UserName", customer.Name);

            return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ
        }

        // Đăng xuất
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("DangNhap");
        }
    }
}
