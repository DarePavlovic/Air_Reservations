using Microsoft.EntityFrameworkCore;
using AirReservationsApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AirReservationsApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<Flight> Flights { get; set; }

        public new DbSet<User> Users { get; set; }

        public DbSet<Reservation> Reservations { get; set; }
    }
}
