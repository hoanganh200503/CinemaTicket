namespace CinemaTicketApp.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public int CustomerId { get; set; }
        public int ShowtimeId { get; set; }
        public decimal Price { get; set; }
        public int SeatId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public Customer Customer { get; set; }
        public Showtime Showtime { get; set; }
        public Seat Seat { get; set; }
    }
}
