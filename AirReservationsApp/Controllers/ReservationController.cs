using System.Security.Claims;
using System.Threading.Tasks;
using AirReservationsApp.Data;
using AirReservationsApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AirReservationsApp.Controllers
{

    public class ReservationController : Controller
    {

        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<User> userManager;
        private readonly IHubContext<ReservationHub> hubContext;

        public ReservationController(ApplicationDbContext dbContext, UserManager<User> userManager, IHubContext<ReservationHub> hubContext)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.hubContext = hubContext;
        }


        [HttpGet]
        [Authorize(Roles = "Viewer")]
        public IActionResult ReserveFlight(int flightId)
        {
            var flight = dbContext.Flights.FirstOrDefault(f => f.Id == flightId);
            if (flight == null || flight.SeatsAvailable <= 0)
            {
                ViewData["ErrorMessage"] = "Invalid flight selection.";
                return RedirectToAction("SearchFlights", "Flight");
            }


            return View(new ReservationViewModel
            {
                FlightId = flightId,
                Flight = flight,   // Set the required Flight object
                Status = "Pending" // Set the required Status value
            });
        }

        [HttpPost]
        [Authorize(Roles = "Viewer")]
        public async Task<IActionResult> ReserveFlight(ReservationViewModel viewModel)
        {
            
            var flight = dbContext.Flights.FirstOrDefault(f => f.Id == viewModel.FlightId);
            if (flight == null || viewModel.SeatsReserved > flight.SeatsAvailable)
            {
                ViewData["ErrorMessage"] = "Not enough seats available.";
                return View(viewModel);
            }

            var userId = userManager.GetUserId(User);

            var reservation = new Reservation
            {
                UserId = userId ?? string.Empty,
                FlightId = viewModel.FlightId,
                SeatsReserved = viewModel.SeatsReserved,
                Flight = flight,
                Status = "Pending"
            };

            // Save reservation
            dbContext.Reservations.Add(reservation);
            //flight.SeatsAvailable -= reservation.SeatsReserved; // This should be done when agent accepts the reservation
            dbContext.SaveChanges();
            await hubContext.Clients.All.SendAsync("ReceiveNewReservation"); 
            return RedirectToAction("MyReservations", "Reservation");
        }

        [HttpGet]
        [Authorize(Roles = "Viewer")]
        public async Task<IActionResult> MyReservations()
        {
            string userId = userManager.GetUserId(User) ?? string.Empty;
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }
            var reservations = await dbContext.Reservations
                                .Where(r => r.UserId == userId)
                                .Select(r => new ReservationsViewModel
                                {
                                    ReservationId = r.Id,
                                    Departure = r.Flight.Departure,
                                    Destination = r.Flight.Destination,
                                    Date = r.Flight.Date,
                                    NumOfReservedSeats = r.SeatsReserved,
                                    Status = r.Status
                                }).OrderBy(r => r.Date).ToListAsync();


            return View(reservations);
        }

        [HttpGet]
        [Authorize(Roles = "Agent")]

        public async Task<IActionResult> ManageReservations()
        {

            var pendingReservations = await dbContext.Reservations
                .Where(r => r.Status == "Pending")
                .Select(r => new ReservationsApprovalViewModel
                {

                    ReservationId = r.Id,
                    UserName = dbContext.Users.Where(u => u.Id == r.UserId).Select(u => u.UserName).FirstOrDefault() ?? string.Empty,
                    Departure = r.Flight.Departure,
                    Destination = r.Flight.Destination,
                    Date = r.Flight.Date,
                    NumOfReservedSeats = r.SeatsReserved,
                    Status = r.Status
                })
                .ToListAsync();

            return View(pendingReservations);
        }

        [HttpPost]
        [Authorize(Roles = "Agent")]

        public async Task<IActionResult> ApproveReservation(int reservationId)
        {
            var reservation = dbContext.Reservations
        .Include(r => r.Flight) // Include flight data
        .FirstOrDefault(r => r.Id == reservationId);

            if (reservation == null || reservation.Flight == null)
            {
                return NotFound(); // Handle invalid reservation
            }

            if (reservation.Status != "Pending")
            {
                return BadRequest("Reservation is already processed.");
            }

            // Check if there are enough seats available
            if (reservation.Flight.SeatsAvailable < reservation.SeatsReserved)
            {
                return BadRequest("Not enough available seats.");
            }

            // Decrease available seats
            reservation.Flight.SeatsAvailable -= reservation.SeatsReserved;

            // Update reservation status to 'Approved'
            reservation.Status = "Approved";

            dbContext.SaveChanges(); // Save changes to DB

            await hubContext.Clients.All.SendAsync("ReceiveReservationStatusChange", reservation.Id, "Approved");
            await hubContext.Clients.All.SendAsync("ReceiveReservationUpdate");


            return RedirectToAction("ManageReservations");
        }

        [HttpPost]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> DeclineReservation(int reservationId)
        {
            var reservation = dbContext.Reservations.Find(reservationId);
            if (reservation != null)
            {
                reservation.Status = "Declined";
                dbContext.SaveChanges();
                await hubContext.Clients.All.SendAsync("ReceiveReservationStatusChange", reservation.Id, "Declined");
                await hubContext.Clients.All.SendAsync("ReceiveReservationUpdate");

            }

            return RedirectToAction("ManageReservations");
        }

    }
}