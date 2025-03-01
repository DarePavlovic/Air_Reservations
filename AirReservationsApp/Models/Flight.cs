using System;
using System.ComponentModel.DataAnnotations;

namespace AirReservationsApp.Models
{
    public class Flight
    {
        public int Id { get; set; }

        public required string Departure { get; set; }

        public required string Destination { get; set; }

        public required DateTime Date { get; set; }

        public required int NumOfConnectedFlights { get; set; }

        public required int SeatsAvailable { get; set; }
    }
}
