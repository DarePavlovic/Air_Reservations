using AirReservationsApp.Controllers;
using AirReservationsApp.Data;
using AirReservationsApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace AirReservationsApp.AirReservationsApp.Tests
{
    public class UnitTest1 : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly FlightController _FlightController;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IHubContext<ReservationHub>> _hubContextMock;
        private readonly Mock<IClientProxy> _mockClientProxy;

        private readonly ReservationController _ReservationController;

        public UnitTest1()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

            _dbContext = new ApplicationDbContext(options);
            _dbContext.Database.EnsureDeleted(); // Ensures clean state before each test
            _dbContext.Database.EnsureCreated();

            _userManagerMock = GetMockUserManager();
            _hubContextMock = new Mock<IHubContext<ReservationHub>>();
            _mockClientProxy = new Mock<IClientProxy>();
            _hubContextMock.Setup(hub => hub.Clients.All).Returns(_mockClientProxy.Object);

            _ReservationController = new ReservationController(
                _dbContext, // Using the real in-memory database
                _userManagerMock.Object,
                _hubContextMock.Object
            );

            _FlightController = new FlightController(_dbContext);
        }

        [Fact]
        public async Task AddFlight_Post_ValidModel_ShouldAddFlight()
        {
            // Arrange: Set up the flight view model and clear model state
            var viewModel = new FlightViewModel
            {
                Departure = "Beograd",
                Destination = "Pristina",
                Date = DateTime.UtcNow.AddDays(5),
                NumOfConnectedFlights = 1,
                SeatsAvailable = 100
            };
            _FlightController.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>()); // Fix TempData issue

            _FlightController.ModelState.Clear();

            // Act: Call the AddFlight method
            var result = await _FlightController.AddFlight(viewModel);


            var flight = _dbContext.Flights.FirstOrDefault(f => f.Departure == "Beograd" && f.Destination == "Pristina");
            Assert.NotNull(flight); // Ensure the flight was added
            Assert.Equal("Beograd", flight.Departure);
            Assert.Equal("Pristina", flight.Destination);
            Assert.Equal(1, flight.NumOfConnectedFlights);
            Assert.Equal(100, flight.SeatsAvailable);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AddFlight", redirectResult.ActionName);
        }

        [Fact]
        public void SearchFlights_ValidSearch_ShouldReturnMatchingFlights()
        {
            // Arrange: Add flights to the in-memory database
            _dbContext.Flights.Add(new Flight
            {
                Departure = "Beograd",
                Destination = "Pristina",
                Date = DateTime.UtcNow.AddDays(2),
                NumOfConnectedFlights = 0,
                SeatsAvailable = 50
            });
            _dbContext.Flights.Add(new Flight
            {
                Departure = "Beograd",
                Destination = "Pristina",
                Date = DateTime.UtcNow.AddDays(3),
                NumOfConnectedFlights = 1,
                SeatsAvailable = 50
            });
            _dbContext.Flights.Add(new Flight
            {
                Departure = "Beograd",
                Destination = "Pristina",
                Date = DateTime.UtcNow.AddDays(4),
                NumOfConnectedFlights = 1,
                SeatsAvailable = 50
            });

            _dbContext.SaveChanges();

            // Act: Call the SearchFlights method
            var result = _FlightController.SearchFlights("Beograd", "Pristina", "0") as ViewResult;
            Assert.NotNull(result);
            var flights = Assert.IsType<List<Flight>>(result.Model);

            Assert.Single(flights);
            Assert.Equal("Beograd", flights[0].Departure);
            Assert.Equal("Pristina", flights[0].Destination);
            Assert.Equal(0, flights[0].NumOfConnectedFlights);
        }

        [Fact]
        public async Task ReserveFlight_ValidModel_ShouldCreateReservation()
        {
            // Arrange
            var flight = AddFlightToDb();
            string userId = GetUserId(); // Get the user ID from Mock UserManager

            var viewModel = new ReservationViewModel { FlightId = 1, SeatsReserved = 2, Flight = flight, Status = "Pending", UserId = userId };
            // Act: Call the ReserveFlight method
            var result = await _ReservationController.ReserveFlight(viewModel);

            // Assert: Verify the reservation was created and the result is a redirect
            var reservation = _dbContext.Reservations.FirstOrDefault(r => r.FlightId == 1);
            Assert.NotNull(reservation);
            Assert.Equal("Pending", reservation.Status);
            Assert.Equal(2, reservation.SeatsReserved);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task ApproveReservation_Valid_ShouldUpdateStatus() //Check if the reservation status is updated to Approved and the number of available seats is reduced
        {
            // Arrange: Add a flight and reservation to the in-memory database
            var flight = AddFlightToDb(10, 0);
            var userId = GetUserId();// Get the user ID from Mock UserManager
            var reservation = AddReservationToDb(flight.Id, flight, 2, userId);

            // Act: Call the ApproveReservation method
            var result = await _ReservationController.ApproveReservation(reservationId: reservation.Id);

            // Assert: Verify the reservation status was updated and seats available were reduced
            var updatedReservation = await _dbContext.Reservations.FindAsync(reservation.Id);
            var updatedFlight = await _dbContext.Flights.FindAsync(flight.Id);

            Assert.NotNull(updatedReservation);
            Assert.Equal("Approved", updatedReservation.Status);
            Assert.NotNull(updatedFlight);
            Assert.Equal(8, updatedFlight.SeatsAvailable); // Ensure seat count is reduced
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task DeclineReservation_Valid_ShouldUpdateStatus()
        {
            // Arrange: Add a flight and reservation to the in-memory database
            var flight = AddFlightToDb(10, 0);
            var userId = GetUserId();// Get the user ID from Mock UserManager
            var reservation = AddReservationToDb(flight.Id, flight, 2, userId);

            // Act: Call the ApproveReservation method
            var result = await _ReservationController.DeclineReservation(reservationId: reservation.Id);

            // Assert: Verify the reservation status was updated and seats available were reduced
            var updatedReservation = await _dbContext.Reservations.FindAsync(reservation.Id);
            var updatedFlight = await _dbContext.Flights.FindAsync(flight.Id);

            Assert.NotNull(updatedReservation);
            Assert.Equal("Declined", updatedReservation.Status);
            Assert.NotNull(updatedFlight);
            Assert.Equal(10, updatedFlight.SeatsAvailable); // Ensure seat count is reduced
            Assert.IsType<RedirectToActionResult>(result);

        }

        [Fact]
        public async Task ReserveFlight_ShouldTriggerSignalR() //Tests the ReserveFlight method of the ReservationController to ensure it triggers SignalR and returns a redirect result.
        {
            // Arrange
            var flight = AddFlightToDb(5,0);
            var userId = GetUserId(); // Get the user ID from Mock UserManager
            var viewModel = new ReservationViewModel { FlightId = 1, SeatsReserved = 2, Flight = flight, Status = "Pending", UserId=userId };

            // Set up SignalR mock
            SetupSignalRMock(); 

            // Act: Call the ReserveFlight method
            var result = await _ReservationController.ReserveFlight(viewModel);

            // Assert: Verify SignalR was triggered and the result is a redirect
            _mockClientProxy.Verify(c => c.SendCoreAsync("ReceiveNewReservation", It.IsAny<object[]>(), default), Times.Once);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task ApproveReservation_ShouldTriggerSignalR()
        {
            // Arrange: Add a flight and reservation to the in-memory database
            var flight = AddFlightToDb(5,0);
            var userId = GetUserId(); // Get the user ID from Mock UserManager
            var reservation = AddReservationToDb(flight.Id, flight, 2, userId);
            
            SetupSignalRMock();

            // Act: Call the ApproveReservation method
            var result = await _ReservationController.ApproveReservation(1);

            // Assert: Verify SignalR was triggered and the result is a redirect
            _mockClientProxy.Verify(c => c.SendCoreAsync("ReceiveReservationStatusChange", It.IsAny<object[]>(), default), Times.Once);
            _mockClientProxy.Verify(c => c.SendCoreAsync("ReceiveReservationUpdate", It.IsAny<object[]>(), default), Times.Once);
            Assert.IsType<RedirectToActionResult>(result);
        }

        private Mock<UserManager<User>> GetMockUserManager()
        {
            // Create a mock UserManager
            var store = new Mock<IUserStore<User>>();
            var mockUserManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            // Set up the GetUserId method to return a test user ID
            mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("test-user-id");

            return mockUserManager;
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted(); // Ensures the database is deleted after each test
            _dbContext.Dispose();
        }
        private string GetUserId()
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "user123") };
            var identity = new ClaimsIdentity(claims);
            var userPrincipal = new ClaimsPrincipal(identity);

            // Mock UserManager to return the expected user ID
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user123");

            // Act: Retrieve user ID
            var userId = _userManagerMock.Object.GetUserId(userPrincipal); // Get the user ID
            if (userId == null)
            {
                userId = "";
            }
            return userId;
        }

        private Flight AddFlightToDb(int seatsAvailable = 10, int connectedFlights = 0)
        {
            var flight = new Flight
            {
                Departure = "Beograd",
                Destination = "Pristina",
                Date = DateTime.UtcNow.AddDays(4),
                NumOfConnectedFlights = connectedFlights,
                SeatsAvailable = seatsAvailable
            };
            _dbContext.Flights.Add(flight);
            _dbContext.SaveChanges();
            return flight;
        }

        private Reservation AddReservationToDb(int flightId, Flight flight, int seatsReserved, string userid)
        {
            var reservation = new Reservation
            {
                FlightId = flightId,
                SeatsReserved = seatsReserved,
                Status = "Pending",
                UserId = userid,
                Flight = flight
            };
            _dbContext.Reservations.Add(reservation);
            _dbContext.SaveChanges();
            return reservation;
        }

        private void SetupSignalRMock()
        {
            //Mock SignalR Hub
            var hubClientsMock = new Mock<IHubClients>(); //A mocked IHubClients object simulates SignalR clients.
            hubClientsMock.Setup(c => c.All).Returns(_mockClientProxy.Object); //_mockClientProxy is set up to capture calls to SendCoreAsync(), which is how SignalR sends messages.
            _hubContextMock.Setup(h => h.Clients).Returns(hubClientsMock.Object);
        }
    }
}