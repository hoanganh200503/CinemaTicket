using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaTicket.Data
{
    public partial class Showtime
    {
        [Key]
        public int ShowtimeId { get; set; }  

        [Required(ErrorMessage = "Vui lòng chọn phim.")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng chiếu.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian chiếu.")]
        public DateTime ShowTime { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

        public virtual Movie? Movie { get; set; }
        public virtual Room? Room { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
