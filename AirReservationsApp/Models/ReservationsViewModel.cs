namespace AirReservationsApp.Models
{
    public class ReservationsViewModel
    {
        public string Departure { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int NumOfReservedSeats { get; set; }
        public string Status { get; set; } = "Pending";
    }
}

