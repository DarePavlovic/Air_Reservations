namespace AirReservationsApp.Models
{
    public class AddUserViewModel
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
        public required string Lastname { get; set; }
        public required string UserType { get; set; }
    }
}