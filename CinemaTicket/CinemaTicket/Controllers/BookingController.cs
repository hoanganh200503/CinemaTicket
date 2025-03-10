using Microsoft.AspNetCore.Mvc;
using CinemaTicket.Data;
using System.Linq;
using Newtonsoft.Json;
using CinemaTicket.Services;
using CinemaTicket.VNPAY;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace CinemaTicket.Controllers
{
    public class BookingController : Controller
    {
        private readonly CinemaTicketDbContext _context;
        private readonly BookingService _bookingService;

        public BookingController(CinemaTicketDbContext context, BookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
        }

        // Hiển thị danh sách suất chiếu
        public IActionResult Index(int movieId)
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
            var movie = _context.Movies.Include(v=>v.MovieGenreMappings).FirstOrDefault(m => m.MovieId == movieId);
            if (movie == null)
            {
                return NotFound("Không tìm thấy phim." + movieId);
            }

            var showtimes = _context.Showtimes
                .Where(s => s.MovieId == movieId)
                .ToList();

            ViewBag.Movie = movie;
            return View(showtimes);
        }


        // Lấy danh sách phòng dựa trên suất chiếu
        [HttpGet]
        public JsonResult GetRooms(int showtimeId)
        {
            var rooms = _context.Rooms
                .Where(r => r.Showtimes.Any(s => s.ShowtimeId == showtimeId))
                .ToList();
            return Json(rooms);
        }

        // Lấy danh sách ghế còn trống theo phòng
        [HttpGet]
        public JsonResult GetSeats(int roomId)
        {
            var seats = _context.Seats
                .Where(s => s.RoomId == roomId)
                .Select(s => new
                {
                    s.SeatId,
                    s.SeatNumber,
                    s.Status,
                    s.SeatType,
                    Price = s.SeatTypeNavigation.Price
                })
                .ToList();

            return Json(seats);
        }



        // Xác nhận đặt vé
        [HttpPost]
        public IActionResult ConfirmBooking([FromBody] List<int> seatIds)
        {
            var seats = _context.Seats.Where(s => seatIds.Contains(s.SeatId)).ToList();

            if (seats.Any(s => s.Status != "Available"))
            {
                return BadRequest("Một số ghế đã được đặt.");
            }

            foreach (var seat in seats)
            {
                seat.Status = "PendingPayment"; 
            }

            _context.SaveChanges();

            HttpContext.Session.SetString("PendingSeats", JsonConvert.SerializeObject(seatIds));

            return Ok("Ghế đã được giữ, tiếp tục thanh toán.");
        }


        [HttpPost("CreatePayment")]
        public IActionResult CreatePayment([FromBody] PaymentRequest request)
        {
            if (request.Amount <= 0 || request.SeatIds == null || request.SeatIds.Count == 0)
            {
                return BadRequest("Thông tin thanh toán không hợp lệ.");
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null)
            {
                return BadRequest("Thông tin thanh toán không hợp lệ.");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.Email == userEmail);
            if (customer == null)
            {
                return BadRequest("Thông tin thanh toán không hợp lệ.");
            }

            string orderId;
            string paymentUrl = CreatePaymentUrl(request.Amount, "Thanh toán vé xem phim", out orderId);

            HttpContext.Session.SetString("OrderId", orderId);
            HttpContext.Session.SetString("PendingSeats", JsonConvert.SerializeObject(request.SeatIds));
            HttpContext.Session.SetInt32("ShowtimeId", request.ShowtimeId);
            HttpContext.Session.SetInt32("CustomerId", customer.CustomerId);

            return Ok(new { PaymentUrl = paymentUrl });
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            var queryParameters = HttpContext.Request.Query;
            VnPayLibrary vnpay = new VnPayLibrary();

            foreach (var param in queryParameters)
            {
                if (param.Key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(param.Key, param.Value);
                }
            }

            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string orderId = HttpContext.Session.GetString("OrderId");
            var seatIdsJson = HttpContext.Session.GetString("PendingSeats");
            int? showtimeId = HttpContext.Session.GetInt32("ShowtimeId");
            int? customerId = HttpContext.Session.GetInt32("CustomerId");

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(seatIdsJson) || !showtimeId.HasValue || !customerId.HasValue)
            {
                TempData["Status"] = "error";
                TempData["Message"] = "Không tìm thấy thông tin đặt vé.";
                return RedirectToAction("PaymentStatus");
            }

            var seatIds = JsonConvert.DeserializeObject<List<int>>(seatIdsJson);

            if (vnp_ResponseCode == "00")
            {
                // Lấy thông tin suất chiếu
                var showtime = await _context.Showtimes
                    .Include(s => s.Movie)
                    .Include(s => s.Room)
                    .FirstOrDefaultAsync(s => s.ShowtimeId == showtimeId.Value);

                if (showtime == null)
                {
                    TempData["Status"] = "error";
                    TempData["Message"] = "Không tìm thấy suất chiếu.";
                    return RedirectToAction("PaymentStatus");
                }

                // Lấy thông tin khách hàng
                var customer = await _context.Customers.FindAsync(customerId.Value);
                if (customer == null)
                {
                    TempData["Status"] = "error";
                    TempData["Message"] = "Không tìm thấy khách hàng.";
                    return RedirectToAction("PaymentStatus");
                }

                // Lấy thông tin ghế ngồi
                var seats = await _context.Seats
                    .Where(s => seatIds.Contains(s.SeatId))
                    .Include(s => s.SeatTypeNavigation)
                    .ToListAsync();

                decimal totalPrice = seats.Sum(s => s.SeatTypeNavigation.Price);

                // Cập nhật trạng thái ghế đã thanh toán
                foreach (var seat in seats)
                {
                    await _bookingService.PurchaseTicket(customerId.Value, seat.SeatId, showtimeId.Value, "Momo");
                }

                // Gửi email xác nhận đặt vé
                string emailBody = $@"
            <h2>Thông tin đặt vé</h2>
            <p><b>Khách hàng:</b> {customer.Name}</p>
            <p><b>Phim:</b> {showtime.Movie.Title}</p>
            <p><b>Phòng chiếu:</b> {showtime.Room.RoomName}</p>
            <p><b>Thời gian:</b> {showtime.Showtime1.ToString("dd/MM/yyyy HH:mm")}</p>
            <p><b>Ghế:</b> {string.Join(", ", seats.Select(s => s.SeatNumber))}</p>
            <p><b>Tổng tiền:</b> {totalPrice} VND</p>
            <p>Cảm ơn bạn đã đặt vé tại rạp chiếu phim của chúng tôi!</p>";
                EmailService _emailService = new EmailService();
                _emailService.SendEmail(customer.Email, "Xác nhận đặt vé thành công", emailBody);

                // Xóa session sau khi thanh toán
                HttpContext.Session.Remove("PendingSeats");
                HttpContext.Session.Remove("OrderId");
                HttpContext.Session.Remove("ShowtimeId");
                HttpContext.Session.Remove("CustomerId");

                TempData["Status"] = "success";
                TempData["Message"] = "Thanh toán thành công! Email xác nhận đã được gửi.";
            }
            else
            {
                var seats = _context.Seats.Where(s => seatIds.Contains(s.SeatId)).ToList();
                foreach (var seat in seats)
                {
                    seat.Status = "Available";
                }
                _context.SaveChanges();

                TempData["Status"] = "error";
                TempData["Message"] = $"Thanh toán thất bại. Mã lỗi: {vnp_ResponseCode}";
            }

            return RedirectToAction("PaymentStatus");
        }

        public IActionResult PaymentStatus()
        {
            return View();
        }

        private static string CreatePaymentUrl(decimal amount, string orderInfo, out string orderId)
        {
            string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            string vnp_TmnCode = "SIS8ACPU";
            string vnp_HashSecret = "PKQCVSGLRUNJLZBSVOQREXNIKZHWPTMW";
            string returnUrl = "https://localhost:7094/Booking/PaymentCallback";

            orderId = DateTime.Now.Ticks.ToString();
            string ipAddress = Utils.GetIpAddress();

            VnPayLibrary vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "billpayment");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId);
            vnpay.AddRequestData("vnp_Locale", "vn");

            return vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        }

    }
}
