using System;
using System.Collections.Generic;

namespace CinemaTicket.Models;

public partial class MovieGenre
{
    public int GenreId { get; set; }

    public string GenreName { get; set; } = null!;

    public virtual ICollection<MovieGenreMapping> MovieGenreMappings { get; set; } = new List<MovieGenreMapping>();
}
