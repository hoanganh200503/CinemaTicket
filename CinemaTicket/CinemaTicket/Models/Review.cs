namespace CinemaTicketApp.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int TicketId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public Ticket Ticket { get; set; }
    }
}
