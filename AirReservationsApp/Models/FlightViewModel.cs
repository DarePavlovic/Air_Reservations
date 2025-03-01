using System;
using System.ComponentModel.DataAnnotations;

namespace AirReservationsApp.Models
{
    public class FlightViewModel
    {
        [Required(ErrorMessage = "Departure is required")]
        [Display(Name = "Departure")]        
        public string Departure { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination is required")]
        [Display(Name = "Destination")]
        public string Destination { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Date")]
        public required DateTime Date { get; set; }
        [Required(ErrorMessage = "Number of connected flights is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of connected flights must be 0 or greater")]
        [Display(Name = "Number of Connected Flights")]
        public required int NumOfConnectedFlights { get; set; }
        [Required(ErrorMessage = "Seats available is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Seats available must be greater than 0")]
        [Display(Name = "Seats Available")]
        public required int SeatsAvailable { get; set; }
    }
}
