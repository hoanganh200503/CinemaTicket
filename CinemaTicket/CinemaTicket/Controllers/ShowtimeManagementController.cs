using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaTicket.Data;
using System.Linq;
using System.Threading.Tasks;


public class ShowtimeManagementController : Controller
{
    private readonly CinemaTicketDbContext _context;

    public ShowtimeManagementController(CinemaTicketDbContext context)
    {
        _context = context;
    }

    // Danh sách suất chiếu
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var totalItems = await _context.Showtimes.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var showtimes = await _context.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Room)
            .OrderBy(s => s.ShowTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentPage = page;

        return View(showtimes);
    }




    // GET: Tạo suất chiếu mới
    public IActionResult Create()
    {
        ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Title");
        ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomName");
        return View();
    }

    // POST: Lưu suất chiếu mới
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Showtime model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Title", model.MovieId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomName", model.RoomId);
            return View(model);
        }

        // Lấy thời lượng phim
        var movie = _context.Movies.Find(model.MovieId);
        if (movie == null)
        {
            ModelState.AddModelError("", "Phim không tồn tại.");
            return View(model);
        }

        int durationMinutes = movie.Duration; // Thời lượng phim
        int breakTime = 10; // 10 phút nghỉ giữa các suất

        DateTime newShowtimeStart = model.ShowTime;
        DateTime newShowtimeEnd = newShowtimeStart.AddMinutes(durationMinutes + breakTime);

        // Kiểm tra suất chiếu bị chồng lên nhau trong cùng phòng
        var isOverlap = _context.Showtimes
     .Where(s => s.RoomId == model.RoomId)
     .Select(s => new
     {
         s.ShowTime,
         MovieDuration = _context.Movies.Where(m => m.MovieId == s.MovieId).Select(m => m.Duration).FirstOrDefault()
     })
     .Any(s =>
         (newShowtimeStart >= s.ShowTime && newShowtimeStart < s.ShowTime.AddMinutes(s.MovieDuration + breakTime)) ||
         (newShowtimeEnd > s.ShowTime && newShowtimeEnd <= s.ShowTime.AddMinutes(s.MovieDuration + breakTime)) ||
         (newShowtimeStart <= s.ShowTime && newShowtimeEnd >= s.ShowTime.AddMinutes(s.MovieDuration + breakTime))
     );

        if (isOverlap)
        {
            ModelState.AddModelError("", "Suất chiếu này bị trùng với suất chiếu khác trong cùng phòng.");
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Title", model.MovieId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomName", model.RoomId);
            return View(model);
        }

        // Nếu không trùng, thêm vào database
        var showtime = new Showtime
        {
            MovieId = model.MovieId,
            RoomId = model.RoomId,
            ShowTime = model.ShowTime,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Showtimes.Add(showtime);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }


    // GET: Chỉnh sửa suất chiếu
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        var showtime = await _context.Showtimes.FindAsync(id);
        ViewBag.MovieId = new SelectList(_context.Movies, "MovieId", "Title", showtime.MovieId);
        ViewBag.RoomId = new SelectList(_context.Rooms, "RoomId", "RoomName", showtime.RoomId);
        return View(showtime);
    }
    // POST: Lưu chỉnh sửa suất chiếu
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ShowtimeId,MovieId,RoomId,ShowTime")] Showtime sh)
    {
        var movie = await _context.Movies.FindAsync(sh.MovieId);
        if (movie == null)
        {
            ModelState.AddModelError("", "Phim không tồn tại.");
            return View(sh);
        }

        int durationMinutes = movie.Duration;
        int breakTime = 10;
        DateTime newShowtimeStart = sh.ShowTime;
        DateTime newShowtimeEnd = newShowtimeStart.AddMinutes(durationMinutes + breakTime);

        // Kiểm tra suất chiếu bị chồng lên nhau (trừ chính nó)
        var isOverlap = _context.Showtimes
    .Where(s => s.ShowtimeId != id && s.RoomId == sh.RoomId)
    .Select(s => new
    {
        s.ShowTime,
        MovieDuration = _context.Movies.Where(m => m.MovieId == s.MovieId).Select(m => m.Duration).FirstOrDefault()
    })
    .Any(s =>
        (newShowtimeStart >= s.ShowTime && newShowtimeStart < s.ShowTime.AddMinutes(s.MovieDuration + breakTime)) ||
        (newShowtimeEnd > s.ShowTime && newShowtimeEnd <= s.ShowTime.AddMinutes(s.MovieDuration + breakTime)) ||
        (newShowtimeStart <= s.ShowTime && newShowtimeEnd >= s.ShowTime.AddMinutes(s.MovieDuration + breakTime))
    );


        if (isOverlap)
        {
            ModelState.AddModelError("", "Suất chiếu này bị trùng với suất chiếu khác trong cùng phòng.");
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Title", sh.MovieId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomName", sh.RoomId);
            return View(sh);
        }

        _context.Update(sh);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    // GET: Xác nhận xóa suất chiếu
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var showtime = await _context.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(m => m.ShowtimeId == id);

        if (showtime == null) return NotFound();

        return View(showtime);
    }

    // POST: Xóa suất chiếu
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var showtime = await _context.Showtimes.FindAsync(id);
        if (showtime != null)
        {
            _context.Showtimes.Remove(showtime);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
