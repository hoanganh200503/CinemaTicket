using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CinemaTicketAdmin.Data;

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

    [DisplayName("Poster")]
    public string? Url { get; set; }

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();

    public virtual ICollection<MovieGenreMapping> MovieGenreMappings { get; set; } = new List<MovieGenreMapping>();
}
