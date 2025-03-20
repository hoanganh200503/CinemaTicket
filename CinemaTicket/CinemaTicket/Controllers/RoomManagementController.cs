using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaTicket.Models;

namespace CinemaTicket.Controllers
{
    public class RoomManagementController : Controller
    {
        private readonly CinemaTicketDbContext _context;

        public RoomManagementController(CinemaTicketDbContext context)
        {
            _context = context;
        }

        // GET: RoomManagement
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rooms
                .Include(r => r.Seats)
                .ToListAsync());
        }
        // Chuyển sang trang quản lý ghế
        public IActionResult GoToSeatManagement(int roomId)
        {
            // Lưu RoomId vào TempData
            TempData["roomid"] = roomId;

            // Chuyển hướng đến SeatsManagementController
            return RedirectToAction("Index", "SeatManagement", new { roomId = roomId });
        }
        // GET: RoomManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: RoomManagement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RoomManagement/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,RoomName,RoomType,Cols,Rows,CreatedAt,UpdatedAt")] Room room)
        {
            if (ModelState.IsValid)
            {
                var existingRoom = _context.Rooms.FirstOrDefault(r => r.RoomName == room.RoomName);
                if (existingRoom != null)
                {
                    ModelState.AddModelError("RoomName", "This room is existed!.");
                    return View(room);
                }
                room.CreatedAt = DateTime.Now;
                room.UpdatedAt = DateTime.Now;

                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: RoomManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: RoomManagement/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,RoomName,RoomType,Cols,Rows,CreatedAt,UpdatedAt")] Room room)
        {
            if (id != room.RoomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRoom = await _context.Rooms.FindAsync(id); // Tìm kiếm theo RoomId
                    if (existingRoom == null)
                    {
                        return NotFound();
                    }
                    // Kiểm tra trùng tên (loại trừ phòng đang chỉnh sửa)
                    if (_context.Rooms.Any(r => r.RoomName == room.RoomName && r.RoomId != id))
                    {
                        ModelState.AddModelError("RoomName", "Room Name Already existed!.");
                        return View(room);
                    }

                    // Cập nhật các thuộc tính của existingRoom
                    existingRoom.RoomName = room.RoomName;
                    existingRoom.Cols = room.Cols;
                    existingRoom.Rows = room.Rows;
                    existingRoom.UpdatedAt = DateTime.Now;

                    _context.Update(existingRoom); // Cập nhật existingRoom
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.RoomId))
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
            return View(room);
        }

        // GET: RoomManagement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.RoomId == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: RoomManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }
    }
}
