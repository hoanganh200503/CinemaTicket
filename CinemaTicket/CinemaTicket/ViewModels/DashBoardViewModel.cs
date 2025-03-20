using CinemaTicket.Models;

namespace CinemaTicket.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalReviews { get; set; }
        public double RatingAverage { get; set; }
        public int TicketPurchased { get; set; }
        public int TotalMembers { get; set; }

        public List<Movie> hotMovies = new List<Movie>();
    }
}
