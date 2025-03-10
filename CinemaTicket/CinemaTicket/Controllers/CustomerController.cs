using Microsoft.AspNetCore.Mvc;
using CinemaTicket.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CinemaTicket.Services;

namespace CinemaTicket.Controllers
{

    public class CustomerController : Controller
    {
        private readonly CinemaTicketDbContext _context;
        private readonly EmailService _emailService;

        public CustomerController(CinemaTicketDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
            if (!IsValidName(customer.Name))
            {
                ModelState.AddModelError("Name", "Tên chỉ được chứa chữ cái, ít nhất 3 ký tự và không để trống.");
                return View(customer);
            }
            if (customer.Password != ConfirmPassword)
            {
                ModelState.AddModelError("Password", "Mật khẩu xác nhận không khớp.");
                return View(customer);
            }
            if (_context.Customers.Any(c => c.Email == customer.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(customer);
            }
            if (!IsValidEmail(customer.Email))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ.");
                return View(customer);
            }
            if (!IsValidPassword(customer.Password))
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 8 ký tự, 1 chữ hoa, 1 số và 1 ký tự đặc biệt.");
                return View(customer);
            }
            customer.Role = "User";
            customer.CreatedAt = DateTime.Now;
            customer.UpdatedAt = DateTime.Now;
            customer.Password = HashPassword(customer.Password);

            try
            {
                _context.Customers.Add(customer);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đăng ký thành công!";
                return RedirectToAction("DangNhap");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã có lỗi xảy ra, vui lòng thử lại!");
                return View(customer);
            }
        }

        // Hiển thị form đăng nhập
        [HttpGet]
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
            HttpContext.Session.SetString("UserRole", customer.Role);

            if (customer.Role == "Admin")
            {
                return RedirectToAction("AdminPage", "Admin", new { area = "Admin" });

            }
            return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ
        }

        // Đăng xuất
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("DangNhap");
        }
        // Trang nhập email để nhận OTP
        public IActionResult ForgotPassword()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == email);
            if (customer == null)
            {
                ViewBag.Error = "Email không tồn tại.";
                return View();
            }

            try
            {
                // 🔥 Tạo OTP
                string otp = new Random().Next(100000, 999999).ToString();

                // 🔥 Lưu OTP vào Session
                HttpContext.Session.SetString("OTP", otp);
                HttpContext.Session.SetString("OTP_Email", email);
                HttpContext.Session.SetString("OTP_Expiry", DateTime.Now.AddMinutes(5).ToString());

                // 🔥 Gửi email OTP
                _emailService.SendEmail(customer.Email, "Mã OTP khôi phục mật khẩu", $"Mã OTP của bạn là: {otp}");

                TempData["SuccessMessage"] = "Mã OTP đã được gửi đến email của bạn.";
                return RedirectToAction("VerifyOTP");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi gửi OTP. Vui lòng thử lại.");
                return View();
            }
        }





        // Trang nhập OTP và mật khẩu mới
        public IActionResult VerifyOTP()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyOTP(string email, string otp, string newPassword)
        {
            string savedOTP = HttpContext.Session.GetString("OTP");
            string savedEmail = HttpContext.Session.GetString("OTP_Email");
            string expiryString = HttpContext.Session.GetString("OTP_Expiry");

            if (string.IsNullOrEmpty(savedOTP) || savedEmail != email || savedOTP != otp)
            {
                ViewBag.Error = "OTP không hợp lệ hoặc đã hết hạn.";
                return View();
            }

            if (DateTime.Parse(expiryString) < DateTime.Now)
            {
                ViewBag.Error = "OTP đã hết hạn.";
                return View();
            }

            // 🔥 Kiểm tra độ mạnh của mật khẩu mới
            if (!IsValidPassword(newPassword))
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 8 ký tự, 1 chữ hoa, 1 số và 1 ký tự đặc biệt.";
                return View();
            }

            try
            {
                var customer = _context.Customers.FirstOrDefault(c => c.Email == email);
                if (customer != null)
                {
                    customer.Password = HashPassword(newPassword);
                    _context.SaveChanges();
                }

                // 🔥 Xóa OTP khỏi Session
                HttpContext.Session.Remove("OTP");
                HttpContext.Session.Remove("OTP_Email");
                HttpContext.Session.Remove("OTP_Expiry");

                TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại. Vui lòng đăng nhập.";
                return RedirectToAction("DangNhap", "Customer");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại.");
                return View();
            }
        }



        public bool IsValidPassword(string password)
        {
            // Kiểm tra độ dài tối thiểu 8 ký tự
            if (password.Length < 8) return false;

            // Kiểm tra ít nhất 1 chữ cái in hoa
            if (!Regex.IsMatch(password, @"[A-Z]")) return false;

            // Kiểm tra ít nhất 1 chữ số
            if (!Regex.IsMatch(password, @"[0-9]")) return false;

            // Kiểm tra ít nhất 1 ký tự đặc biệt (mở rộng danh sách)
            if (!Regex.IsMatch(password, @"[\W_]")) return false;

            return true;
        }

        public bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (name.Length < 3) return false;
            if (!Regex.IsMatch(name, @"^[\p{L} ]+$")) return false;
            return true;
        }

        public bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }
    }
}
