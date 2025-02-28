using System;
using System.Collections.Generic;

namespace CinemaTicketAdmin.Data;

public partial class MovieGenreMapping
{
    public int MovieId { get; set; }
    public int GenreId { get; set; }

    public virtual Movie Movie { get; set; } = null!;
    public virtual MovieGenre Genre { get; set; } = null!;
}
