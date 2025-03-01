using System.ComponentModel.DataAnnotations;
namespace AirReservationsApp.Models
{
    public class LoginViewModel
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Password { get; set; }
        [Display(Name = "Remember me?")]
        public required bool RememberMe { get; set; }
    }
}