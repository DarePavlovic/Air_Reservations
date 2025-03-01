namespace AirReservationsApp.Models
{
    public class ReservationsApprovalViewModel
    {
        public int ReservationId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Departure { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int NumOfReservedSeats { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}