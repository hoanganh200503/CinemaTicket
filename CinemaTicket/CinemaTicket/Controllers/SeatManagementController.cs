using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaTicket.ViewModels;
using CinemaTicket.Models;

namespace CinemaTicket.Controllers
{
    public class SeatManagementController : Controller
    {
        private readonly CinemaTicketDbContext _context;

        public SeatManagementController(CinemaTicketDbContext context)
        {
            _context = context;
        }

        // GET: SeatManagement
        public async Task<IActionResult> Index(int? roomId)
        {
            if (!roomId.HasValue)
            {
                roomId = Convert.ToInt32(TempData.Peek("roomid"));
            }
            else
            {
                TempData["roomid"] = roomId;
            }

            var seats = await _context.Seats
                .Include(s => s.Room)
                .Include(s => s.SeatType)
                .Where(r => r.RoomId == roomId)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();

            ViewBag.RoomId = roomId;
            return View(seats);
        }

        // GET: SeatManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seat = await _context.Seats
                .Include(s => s.Room)
                .Include(s => s.SeatType)
                .FirstOrDefaultAsync(m => m.SeatId == id);
            if (seat == null)
            {
                return NotFound();
            }

            return View(seat);
        }

        // GET: SeatManagement/Create
        public IActionResult Create(int? roomId)
        {
            if (!roomId.HasValue)
            {
                roomId = Convert.ToInt32(TempData.Peek("roomid"));
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomName");
            if (ViewData["RoomId"] == null)
            {
                ViewData["RoomId"] = new List<SelectListItem>();
            }
            ViewData["SeatTypeId"] = new SelectList(_context.SeatTypes, "SeatTypeId", "TypeName");
            if (ViewData["SeatTypeId"] == null)
            {
                ViewData["SeatTypeId"] = new List<SelectListItem>();
            }
            ViewData["Status"] = new SelectList(_context.Seats.Distinct(), "Status", "Status");
            if (ViewData["Status"] == null)
            {
                ViewData["Status"] = new List<SelectListItem>();
            }

            return View();
        }

        // POST: SeatManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? roomId,[Bind("SeatId,RoomId,SeatNumber,SeatTypeId,Status")] Seat seat)
        {
            if (ModelState.IsValid)
            {
                seat.RoomId = (int)roomId;
                seat.CreatedAt = DateTime.Now;
                seat.UpdatedAt = DateTime.Now;
                _context.Add(seat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { roomId = seat.RoomId });
            }
            ViewBag.RoomId = seat.RoomId;
            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomName", seat.RoomId);
            ViewData["SeatTypeId"] = new SelectList(_context.SeatTypes, "SeatTypeId", "TypeName", seat.SeatTypeId);
            ViewData["Status"] = new SelectList(_context.Seats.Distinct(), "Status", "Status",seat.Status);
            return View(seat);
        }

        // GET: SeatManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seat = await _context.Seats.FindAsync(id);
            if (seat == null)
            {
                return NotFound();
            }
            ViewBag.RoomId = seat.RoomId;
            ViewData["SeatTypeId"] = new SelectList(_context.SeatTypes, "SeatTypeId", "TypeName", seat.SeatTypeId);
            return View(seat);
        }

        // POST: SeatManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SeatId,RoomId,SeatNumber,Status,CreatedAt,SeatTypeId")] Seat seat)
        {
            if (id != seat.SeatId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    seat.UpdatedAt = DateTime.Now;
                    _context.Update(seat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeatExists(seat.SeatId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { roomId = seat.RoomId });
            }
            ViewBag.RoomId = seat.RoomId;
            ViewData["SeatTypeId"] = new SelectList(_context.SeatTypes, "SeatTypeId", "TypeName", seat.SeatTypeId);
            return View(seat);
        }

        // GET: SeatManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seat = await _context.Seats
                .Include(s => s.Room)
                .Include(s => s.SeatType)
                .FirstOrDefaultAsync(m => m.SeatId == id);
            if (seat == null)
            {
                return NotFound();
            }

            return View(seat);
        }

        // POST: SeatManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seat = await _context.Seats.FindAsync(id);
            if (seat == null)
            {
                return NotFound();
            }

            int roomId = seat.RoomId;
            _context.Seats.Remove(seat);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { roomId });
        }

        private bool SeatExists(int id)
        {
            return _context.Seats.Any(e => e.SeatId == id);
        }
    }
}
