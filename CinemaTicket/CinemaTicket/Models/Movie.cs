using System.ComponentModel;

/// <summary>
/// Ràng buộc Language tránh lỗi với sql
/// Đặt ngoài namespace giúp sử dụng mà không cần khai báo using từ namespace
/// </summary>
public enum LanguageOption
{
    Subtitled,
    Dubbed
}
namespace CinemaTicketApp.Models
{

    public class Movie
    {
        public LanguageOption enumLanguage;// cái này để chuyển đổi giá trị của language khi lưu hoặc thay đổi

        public int MovieID { get; set; }
        //[DisplayName("Movie name")]
        public string Title { get; set; }
        public int Duration { get; set; }
        public string Language { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
