using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirReservationsApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}