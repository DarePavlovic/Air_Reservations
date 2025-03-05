using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirReservationsApp.Controllers
{
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {
        [HttpGet]
        [Authorize(Roles = "Agent")]
        public IActionResult Index()
        {
            return View();
        }
    }
}