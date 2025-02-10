namespace CinemaTicketApp.Models
{
    public class Review
    {
        public int ReviewID { get; set; }
        public int TicketID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public Ticket Ticket { get; set; }
    }
}
