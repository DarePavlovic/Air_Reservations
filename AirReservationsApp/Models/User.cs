using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AirReservationsApp.Models
{
    public class User:IdentityUser
    {
        // public int Id { get; set; }

        // public required string Username { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Last Name")]
        public string Lastname { get; set; } = string.Empty;
        
        [Display(Name = "Type of User")]
        public required string UserType { get; set; } = "Viewer";

    }
}
