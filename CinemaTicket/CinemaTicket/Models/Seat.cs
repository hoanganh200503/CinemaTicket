namespace CinemaTicketApp.Models
{
    public class Seat
    {
        public int SeatID { get; set; }
        public int RoomID { get; set; }
        public string SeatNumber { get; set; }
        public string SeatType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public Room Room { get; set; }
    }
}
