using System;
using System.Linq;
using System.Threading.Tasks;
using CinemaTicket.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicket.Services
{
    public class BookingService
    {
        private readonly CinemaTicketDbContext _context;

        public BookingService(CinemaTicketDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetSeatPrice(int seatId)
        {
            var seat = await _context.Seats.Include(s => s.SeatType)
                                           .FirstOrDefaultAsync(s => s.SeatId == seatId);
            if (seat == null) throw new Exception("Seat not found.");

            return seat.SeatType.Price;
        }

        public async Task<bool> PurchaseTicket(int customerId, int seatId, int showtimeId, string paymentMethod)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            if (seat == null || seat.Status != "Available")
            {
                throw new Exception("Seat is not available.");
            }

            var price = await GetSeatPrice(seatId);
            var ticket = new Ticket
            {
                CustomerId = customerId,
                ShowtimeId = showtimeId,
                SeatId = seatId,
                Price = price,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var transaction = new Transaction
            {
                PaymentMethod = paymentMethod,
                TotalAmount = price,
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var transactionTicket = new TransactionTicket
            {
                TransactionId = transaction.TransactionId,
                TicketId = ticket.TicketId
            };

            _context.TransactionTickets.Add(transactionTicket);
            await _context.SaveChangesAsync();

            seat.Status = "Reserved";
            _context.Seats.Update(seat);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Ticket?> GetTicketDetails(int ticketId)
        {
            return await _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Showtime)
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }

        public async Task<Customer?> GetCustomerById(int customerId)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }
    }
}