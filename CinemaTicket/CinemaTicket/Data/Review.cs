using System;
using System.Collections.Generic;

namespace CinemaTicket.Data;

public partial class Review
{
    public int ReviewId { get; set; }
    public int MovieId { get; set; }
    public int CustomerId { get; set; }
    public string IsApproved { get; set; } = null!;

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    //  public virtual Ticket Ticket { get; set; } = null!; không liên kết khóa ngoại trong db được nên xóa
    public virtual Movie Movie { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
}
