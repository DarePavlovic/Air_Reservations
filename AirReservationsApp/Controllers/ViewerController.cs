using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirReservationsApp.Controllers
{
    [Authorize(Roles = "Viewer")]
    public class ViewerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}