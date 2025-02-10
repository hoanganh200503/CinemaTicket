namespace CinemaTicketApp.Models
{
    public class Showtime
    {
        public int ShowtimeID { get; set; }
        public int MovieID { get; set; }
        public int RoomID { get; set; }
        public DateTime ShowtimeDate { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public Movie Movie { get; set; }
        public Room Room { get; set; }
    }
}
