using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaTicket.Data;

namespace CinemaTicket.Controllers
{
    public class ReviewManagementController : Controller
    {
        private readonly CinemaTicketDbContext _context;

        public ReviewManagementController(CinemaTicketDbContext context)
        {
            _context = context;
        }

        // GET: ReviewManagement
        public async Task<IActionResult> Index()
        {
            // giá trị mặc định cho dropdownlist IsApproved
            //ViewData["IsApprovedList"] = new SelectList(new List<string> { "Approved", "Denied", "Waiting" });

            // danh sách review chưa được duyệt
            var cinemaTicketDbContext = _context.Reviews.Include(r => r.Customer).Include(r => r.Movie).Where(i => i.IsApproved == "Waiting").OrderByDescending(i => i.CreatedAt);
            return View(await cinemaTicketDbContext.ToListAsync());
        }
        // Chấp nhận Review
        public IActionResult Approve(int id)
        {
            var review = _context.Reviews.Find(id);
            review.IsApproved = "Approved";
            review.UpdatedAt = DateTime.Now;
            _context.Reviews.Update(review);
            _context.SaveChanges();
            return RedirectToAction("ApprovedList");
        }
        // Từ chối Review
        public IActionResult Deny(int id)
        {
            var review = _context.Reviews.Find(id);
            review.IsApproved = "Denied";
            review.UpdatedAt = DateTime.Now;
            _context.Reviews.Update(review);
            _context.SaveChanges();
            return RedirectToAction("DeniedList"); 
        }
        // List Review đã được duyệt
        public async Task<IActionResult> ApprovedList()
        {
            var cinemaTicketDbContext = _context.Reviews.Include(r => r.Customer).Include(r => r.Movie).Where(i => i.IsApproved == "Approved").OrderByDescending(i => i.UpdatedAt);
            return View(await cinemaTicketDbContext.ToListAsync());
        }
        // List Review đã từ chối
        public async Task<IActionResult> DeniedList()
        {
            var cinemaTicketDbContext = _context.Reviews.Include(r => r.Customer).Include(r => r.Movie).Where(i => i.IsApproved == "Denied").OrderByDescending(i => i.UpdatedAt);
            return View(await cinemaTicketDbContext.ToListAsync());
        }
        // GET: ReviewManagement/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId");
            return View();
        }

        // POST: ReviewManagement/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReviewId,TicketId,MovieId,CustomerId,IsApproved,Rating,Comment,CreatedAt,UpdatedAt")] Review review)
        {
            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", review.CustomerId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", review.MovieId);
            return View(review);
        }

        // GET: ReviewManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", review.CustomerId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", review.MovieId);
            return View(review);
        }

        // POST: ReviewManagement/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,TicketId,MovieId,CustomerId,IsApproved,Rating,Comment,CreatedAt,UpdatedAt")] Review review)
        {
            if (id != review.ReviewId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.ReviewId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", review.CustomerId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "MovieId", review.MovieId);
            return View(review);
        }

        // GET: ReviewManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: ReviewManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }
    }
}
