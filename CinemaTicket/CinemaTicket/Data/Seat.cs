using System;
using System.Collections.Generic;

namespace CinemaTicket.Data;

public partial class Seat
{
    public int SeatId { get; set; }

    public int RoomId { get; set; }

    public string SeatNumber { get; set; } = null!;

    public string SeatType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
