using System;
using System.Collections.Generic;

namespace CinemaTicket.Data;

public partial class SeatType
{
    public int SeatTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
