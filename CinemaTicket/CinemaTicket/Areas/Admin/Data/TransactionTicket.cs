using System;
using System.Collections.Generic;

namespace CinemaTicketAdmin.Data;

public partial class TransactionTicket
{
    public int TransactionTicketId { get; set; }

    public int TransactionId { get; set; }

    public int TicketId { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
