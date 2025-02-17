using System.ComponentModel.DataAnnotations;

namespace CinemaTicketApp.Models
{
    public class Customer
    {
 
        public int CustomerID { get; set; }

        [Display(Name = "Họ Tên")]
        [Required(ErrorMessage = "*")]
        [MaxLength(225, ErrorMessage = "Tối đa 225 kí tự")]
        public string Name { get; set; }

        [Display(Name = "Email")]
        [EmailAddress( ErrorMessage = "Chưa đúng định dạng email")]
        public string Email { get; set; }

        [Display(Name = "Mật Khẩu")]
        [Required(ErrorMessage = "*")]
        [DataType(DataType.Password)]   
        public string Password { get; set; }

        public string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
