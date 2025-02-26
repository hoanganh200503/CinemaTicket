namespace CinemaTicketApp.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
