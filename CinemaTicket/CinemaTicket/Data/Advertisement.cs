using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CinemaTicket.Data
{
    public class Advertisement
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }

        // Navigation property
        public virtual Movie? Movie { get; set; }
    }


}
