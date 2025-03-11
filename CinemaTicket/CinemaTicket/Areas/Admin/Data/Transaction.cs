using System;
using System.Collections.Generic;

namespace CinemaTicketAdmin.Data;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<TransactionTicket> TransactionTickets { get; set; } = new List<TransactionTicket>();
}
