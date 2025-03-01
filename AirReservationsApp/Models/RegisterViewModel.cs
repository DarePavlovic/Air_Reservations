using System.ComponentModel.DataAnnotations;
namespace AirReservationsApp.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public required string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Lastname { get; set; } = string.Empty;
        [Required]
        public string UserType { get; set; } = "Viewer";
    }
}