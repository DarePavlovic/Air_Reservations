using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirReservationsApp.Models
{
    public class ReservationViewModel
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;// Reference to the User

        [Required]
        public int FlightId { get; set; } // Reference to the Flight

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Seats must be at least 1.")]
        public int SeatsReserved { get; set; }

        [ForeignKey("FlightId")]
        public required Flight Flight { get; set; }

        public required string Status { get; set; } = "Pending";
    }
}