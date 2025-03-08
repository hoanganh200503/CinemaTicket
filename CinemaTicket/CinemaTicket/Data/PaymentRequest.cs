namespace CinemaTicket.Data
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public List<int> SeatIds { get; set; }
        public int ShowtimeId { get; set; }
        public int CustomerId { get; set; }
    }

}
