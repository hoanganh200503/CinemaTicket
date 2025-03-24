using Microsoft.AspNetCore.Mvc;
using CinemaTicket.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CinemaTicketApp.Controllers     //Đổi lại namespace (cũ  CinemaTicketApp.Controllers)
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

        [HttpGet("Account/Edit/{changePassword?}")]
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

            // Giữ trạng thái đổi mật khẩu nếu có lỗi
            if (TempData.ContainsKey("OpenPasswordChange"))
            {
                changePassword = (bool)TempData["OpenPasswordChange"];
            }

            ViewData["OpenPasswordChange"] = changePassword;
            return View(customer);
        }

        private string GetMd5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                foreach (var t in hashBytes)
                {
                    sb.Append(t.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        [HttpPost]
        public IActionResult Edit(string name, string email, string currentPassword, string newPassword, string changePassword)
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

            // Kiểm tra email hợp lệ bằng Regex
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s\.]+$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                ModelState.AddModelError("email", "Email không hợp lệ! Định dạng đúng: example@domain.com");
                return View(customer);
            }

            // Cập nhật thông tin cơ bản
            customer.Name = name;
            customer.Email = email;

            // Kiểm tra nếu người dùng thực sự muốn đổi mật khẩu
            if (changePassword == "yes")
            {
                ViewData["OpenPasswordChange"] = true; // Giữ trạng thái hiển thị mật khẩu khi có lỗi

                if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
                {
                    ModelState.AddModelError("", "Vui lòng nhập cả mật khẩu hiện tại và mật khẩu mới");
                    return View(customer);
                }

                // Kiểm tra độ mạnh của mật khẩu
                string passwordPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";
                if (!Regex.IsMatch(newPassword, passwordPattern))
                {
                    ModelState.AddModelError("newPassword", "Mật khẩu phải có ít nhất 8 ký tự, 1 chữ hoa, 1 số và 1 ký tự đặc biệt.");
                    return View(customer);
                }

                var currentHashedPassword = GetMd5Hash(currentPassword);

                if (customer.Password != currentHashedPassword)
                {
                    ModelState.AddModelError("currentPassword", "Mật khẩu hiện tại không đúng");
                    return View(customer);
                }

                var newHashedPassword = GetMd5Hash(newPassword);

                if (newHashedPassword == customer.Password)
                {
                    ModelState.AddModelError("newPassword", "Mật khẩu mới không được trùng với mật khẩu cũ.");
                    return View(customer);
                }

                customer.Password = GetMd5Hash(newPassword);
            }

            customer.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            // Cập nhật session
            HttpContext.Session.SetString("CustomerName", customer.Name);
            HttpContext.Session.SetString("CustomerEmail", customer.Email);

            // Gửi thông báo thành công
            TempData["SuccessMessage"] = "Đã cập nhật thông tin thành công!";

            return RedirectToAction("Edit", new { changePassword = false });
        }

        [HttpGet]
        public IActionResult TransactionHistory()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");
            if (userEmail == null)
            {
                return RedirectToAction("DangNhap", "Customer");
            }

            // Lấy CustomerId của khách hàng hiện tại
            var customer = _context.Customers.FirstOrDefault(c => c.Email == userEmail);
            if (customer == null)
            {
                return NotFound();
            }
            // Lấy thông tin giao dịch và vé liên quan từ cơ sở dữ liệu
            var transactions = _context.Transactions
                .Include(t => t.TransactionTickets) // Bao gồm vé liên quan
                .ThenInclude(tt => tt.Ticket) // Bao gồm thông tin vé
                .ThenInclude(t => t.Showtime) // Bao gồm thông tin suất chiếu
                .ThenInclude(s => s.Movie) // Bao gồm thông tin phim
                .Include(t => t.TransactionTickets) // Bao gồm vé liên quan
                .ThenInclude(tt => tt.Ticket) // Bao gồm thông tin vé
                .ThenInclude(t => t.Seat) // Bao gồm thông tin ghế
                .ThenInclude(s => s.Room) // Bao gồm thông tin phòng
                .Where(t => t.TransactionTickets.Any(ticket => ticket.Ticket.CustomerId == customer.CustomerId)) // Lọc theo CustomerId
                .ToList();

            return View(transactions);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
