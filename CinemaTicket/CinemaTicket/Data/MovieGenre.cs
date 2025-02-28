using System;
using System.Collections.Generic;

namespace CinemaTicket.Data
{

    public partial class MovieGenre
    {
        public int GenreId { get; set; }

        public string GenreName { get; set; } = null!;

        public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}