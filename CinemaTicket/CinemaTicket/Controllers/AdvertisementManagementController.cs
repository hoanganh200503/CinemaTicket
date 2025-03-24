using Microsoft.AspNetCore.Mvc;
using CinemaTicket.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaTicket.Controllers
{
    public class AdvertisementManagementController : Controller
    {
        private readonly CinemaTicketDbContext _context;

        public AdvertisementManagementController(CinemaTicketDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách banner với hình ảnh lấy từ Movie
        public async Task<IActionResult> Index()
        {

            var Advertisement = await _context.Advertisements
                .Include(a => a.Movie) // Load Movie liên quan
                 .ToListAsync();
            return View(Advertisement);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy quảng cáo từ bảng Advertisements
            var advertisement = await _context.Advertisements
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(ad => ad.Id == id);

            if (advertisement == null)
            {
                return NotFound();
            }

            return View(advertisement);
        }


        // GET: Banner/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Movies = await _context.Movies.ToListAsync();
            return View(new Advertisement()); // Truyền model rỗng vào view
        }

        // POST: Advertisement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Advertisement advertisement, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng lặp quảng cáo cho cùng một bộ phim
                var existingAd = await _context.Advertisements
                    .FirstOrDefaultAsync(a => a.MovieId == advertisement.MovieId);
                if (existingAd != null)
                {
                    ModelState.AddModelError("", "Advertisement for this movie already exists!");
                    ViewBag.Movies = await _context.Movies.ToListAsync();
                    return View(advertisement);
                }

                if (advertisement.StartDate > advertisement.EndDate)
                {
                    ModelState.AddModelError("", "Start date must be less than end date.");
                    ViewBag.Movies = await _context.Movies.ToListAsync();
                    return View(advertisement);
                }

                // Xử lý upload ảnh
                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    advertisement.ImageUrl = fileName;
                }
                //else
                //{
                //    ModelState.AddModelError("ImageUrl", "Please upload an advertisement image.");
                //    ViewBag.Movies = await _context.Movies.ToListAsync();
                //    return View(advertisement);
                //}




                _context.Advertisements.Add(advertisement);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Movies = await _context.Movies.ToListAsync();
            return View(advertisement);
        }

        // GET: Advertisement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisements              
                .FirstOrDefaultAsync(a => a.Id == id);

            if (advertisement == null)
            {
                return NotFound();
            }

            // Lấy danh sách Movies để hiển thị trong dropdown
            ViewBag.MovieList = new SelectList(_context.Movies, "MovieId", "Title", advertisement.MovieId);

            return View(advertisement);
        }


        // POST: Banner/Edit/5
        // POST: Advertisement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Advertisement advertisement, IFormFile? file)
        {
            if (id != advertisement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAd = await _context.Advertisements
                        .FirstOrDefaultAsync(a => a.Id == id); // ❌ Bỏ Include(a => a.Movie)

                    if (existingAd == null) return NotFound();

                    if (advertisement.StartDate > advertisement.EndDate)
                    {
                        ModelState.AddModelError("", "Start date must be less than end date.");
                        return View(advertisement);
                    }

                    // Cập nhật dữ liệu
                    existingAd.StartDate = advertisement.StartDate;
                    existingAd.EndDate = advertisement.EndDate;
                    existingAd.IsActive = advertisement.IsActive;

                    // Cập nhật ảnh nếu có file mới
                    if (file != null && file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        existingAd.ImageUrl = fileName;
                    }

                    _context.Update(existingAd);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Advertisements.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
            }

            return View(advertisement);
        }




        // GET: Admin/Advertisements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisements
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (advertisement == null)
            {
                return NotFound();
            }

            return View(advertisement);
        }



        // POST: Admin/Advertisements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement != null)
            {
                _context.Advertisements.Remove(advertisement);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
