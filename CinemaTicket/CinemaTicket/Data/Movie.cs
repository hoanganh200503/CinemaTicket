using System;
using System.Collections.Generic;

namespace CinemaTicket.Data;

public partial class Movie
{
    public int MovieId { get; set; }

    public string Title { get; set; } = null!;

    public int Duration { get; set; }

    public string Language { get; set; } = null!;

    public DateOnly ReleaseDate { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();

    public virtual ICollection<MovieGenre> Genres { get; set; } = new List<MovieGenre>();
}
