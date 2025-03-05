using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirReservationsApp.Controllers
{
    [Authorize(Roles = "Viewer")]
    public class ViewerController : Controller
    {
        [HttpGet]
        [Authorize(Roles="Viewer")]
        public IActionResult Index()
        {
            return View();
        }
    }
}