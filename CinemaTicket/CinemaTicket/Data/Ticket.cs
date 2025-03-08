using System;
using System.Collections.Generic;

namespace CinemaTicket.Data;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int CustomerId { get; set; }

    public int ShowtimeId { get; set; }

    public int SeatId { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? PromoCodes { get; set; }

    public decimal? Discount { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Seat Seat { get; set; } = null!;

    public virtual Showtime Showtime { get; set; } = null!;

    public virtual ICollection<TransactionTicket> TransactionTickets { get; set; } = new List<TransactionTicket>();
}
