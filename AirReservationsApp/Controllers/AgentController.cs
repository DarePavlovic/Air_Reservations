using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirReservationsApp.Controllers
{
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}