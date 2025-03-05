using AirReservationsApp.Data;
using AirReservationsApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirReservationsApp.Controllers
{

    public class FlightController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public FlightController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [Authorize(Roles = "Agent")]
        public IActionResult AddFlight()
        {
            return View();
        }


        
        [HttpPost]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> AddFlight(FlightViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Flight flight = new Flight
                {
                    Departure = viewModel.Departure,
                    Destination = viewModel.Destination,
                    Date = viewModel.Date,
                    NumOfConnectedFlights = viewModel.NumOfConnectedFlights,
                    SeatsAvailable = viewModel.SeatsAvailable
                };
                Console.WriteLine("Adding flight");
                await dbContext.Flights.AddAsync(flight);
                dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Flight added successfully!";
                return RedirectToAction("AddFlight");
            }
            return View(viewModel);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            // Retrieve the flight by its ID
            var flight = await dbContext.Flights.FirstOrDefaultAsync(f => f.Id == id);

            // If no flight found, return a NotFound view
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight); // Pass the flight to the view to confirm deletion
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flight = await dbContext.Flights.FirstOrDefaultAsync(f => f.Id == id);

            if (flight == null)
            {
                return NotFound();
            }

            dbContext.Flights.Remove(flight);

            await dbContext.SaveChangesAsync();

            return RedirectToAction("ViewFlights");
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Agent")]
        public async Task<IActionResult> ViewFlights()
        {
            var flights = await dbContext.Flights.OrderBy(f => f.Date).ToListAsync();
            return View(flights);
        }

        [HttpGet]
        [Authorize(Roles="Viewer")]
        public IActionResult SearchFlights(string departure, string destination, string numOfLayovers)
        {
            if (string.IsNullOrEmpty(departure) || string.IsNullOrEmpty(destination))
            {
                ViewData["ErrorMessage"] = "Please fill in all fields and try again.";
                return View(new List<Flight>());
            }

            var flightsQuery = dbContext.Flights.AsQueryable();
            flightsQuery = flightsQuery.Where(f => f.Date >= DateTime.Now);

            if (!string.IsNullOrEmpty(departure))
            {
                flightsQuery = flightsQuery.Where(f => f.Departure == departure);
            }

            if (!string.IsNullOrEmpty(destination))
            {
                flightsQuery = flightsQuery.Where(f => f.Destination == destination);
            }

            if (numOfLayovers == "0")
            {
                flightsQuery = flightsQuery.Where(f => f.NumOfConnectedFlights == 0);
            }

            var flights = flightsQuery.Where(f => f.SeatsAvailable > 0).OrderBy(f => f.Date).ToList();


            if (flights.Count == 0)
            {
                ViewData["NoFlightsMessage"] = "No available flights found matching your search criteria.";
            }

            ViewData["SelectedDeparture"] = departure;
            ViewData["SelectedDestination"] = destination;
            ViewData["SelectedLayovers"] = numOfLayovers;

            return View(flights);
        }



    }
}