using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace CinemaTicket.ViewModels
{
    public class SeatViewModel
    {
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Please enter Seat Number")]
        public string SeatNumber { get; set; }

        [Required(ErrorMessage = "Please choose status")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Please choose Seatype")]
        public int SeatTypeId { get; set; }

        public List<SelectListItem>? AvailableRooms { get; set; }
        public List<SelectListItem>? SeatTypes { get; set; }
    }
}
//Scaffold-DbContext "Data Source=.;Database=CinemaTicketDB;Integrated Security=True;Trust Server Certificate=True" Microsoft.EntityFrameworkCore.SqlServer - OutputDir Models - Context CinemaTicketDB - Force