using CinemaTicket.Data;

namespace CinemaTicket.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalReviews { get; set; }
        public double RatingAverage { get; set; }
        public int TicketPurchased { get; set; }
        public int TotalMembers { get; set; }
            public List<string> GenreLabels { get; set; } = new List<string>();  // Danh sách thể loại
            public List<int> GenreCounts { get; set; } = new List<int>();  // Số lượng phim từng thể loại
        public List<MovieViewModel> hotMovies { get; set; } = new List<MovieViewModel>(); // danh sach phim có avrrating cao nhất


    }

    public class MovieViewModel // chứa dữ liệu của các movie trong hot movie
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public double MovieRatingAverage { get; set; } 
    }
}
 