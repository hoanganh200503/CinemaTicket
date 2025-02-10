namespace CinemaTicketApp.Models
{
    public class Ticket
    {
        public int TicketID { get; set; }
        public int CustomerID { get; set; }
        public int ShowtimeID { get; set; }
        public int SeatID { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public Customer Customer { get; set; }
        public Showtime Showtime { get; set; }
        public Seat Seat { get; set; }
    }
}
