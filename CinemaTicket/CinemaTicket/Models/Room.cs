namespace CinemaTicketApp.Models
{
    public class Room
    {
        public int RoomID { get; set; }
        public string RoomName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
