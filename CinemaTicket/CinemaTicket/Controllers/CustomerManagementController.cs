using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaTicket.Data;

namespace CinemaTicket.Controllers
{
    public class CustomerManagementController : Controller
    {
        private readonly CinemaTicketDbContext _context;

        public CustomerManagementController(CinemaTicketDbContext context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Customers.ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,Name,Email,Password,Role,CreatedAt,UpdatedAt")] Customer customer)
        {
            // Kiểm tra email hợp lệ bằng Regex
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s\.]+$";
            if (!Regex.IsMatch(customer.Email, emailPattern))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ! Định dạng đúng: example@domain.com");
                return View(customer);
            }

            // Kiểm tra email đã tồn tại
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customer.Email);
            if (existingCustomer != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng. Vui lòng sử dụng email khác.");
                return View(customer);
            }

            // Kiểm tra độ mạnh của mật khẩu
            string passwordPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";
            if (!Regex.IsMatch(customer.Password, passwordPattern))
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 8 ký tự, 1 chữ hoa, 1 số và 1 ký tự đặc biệt.");
                return View(customer);
            }

            if (ModelState.IsValid)
            {
                // Hash the password using MD5
                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.ASCII.GetBytes(customer.Password);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);
                    
                    // Convert the byte array to lowercase hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }
                    customer.Password = sb.ToString();
                }

                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,Name,Email,Password,Role,CreatedAt,UpdatedAt")] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }
            // 🟢 Kiểm tra email hợp lệ bằng Regex
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s\.]+$";
            if (!Regex.IsMatch(customer.Email, emailPattern))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ! Định dạng đúng: example@domain.com");
                return View(customer);
            }

            // 🟢 Kiểm tra email đã tồn tại (trừ trường hợp chính customer đó đang sửa email của mình)
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == customer.Email && c.CustomerId != customer.CustomerId);

            if (existingCustomer != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng. Vui lòng sử dụng email khác.");
                return View(customer);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 🟢 Lấy CreatedAt từ database để tránh bị ghi đè
                    var dbCustomer = await _context.Customers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.CustomerId == id);

                    if (dbCustomer != null)
                    {
                        customer.CreatedAt = dbCustomer.CreatedAt; // Giữ nguyên CreatedAt
                    }

                    customer.UpdatedAt = DateTime.UtcNow; // Cập nhật UpdatedAt

                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
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
            return View(customer);
        }




        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
