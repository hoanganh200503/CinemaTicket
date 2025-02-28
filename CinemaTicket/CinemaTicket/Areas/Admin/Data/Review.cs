﻿using System;
using System.Collections.Generic;

namespace CinemaTicketAdmin.Data;

public partial class Review
{
    public int ReviewId { get; set; }

    public int TicketId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
