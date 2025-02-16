using System;
using System.Collections.Generic;

namespace CinemaTicket.Data;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string Title { get; set; } = null!;

    public decimal DiscountPercentage { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
