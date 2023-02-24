using Microsoft.AspNetCore.Mvc;

namespace NEMS.Controllers
{
    public class ClockController : Controller
    {
        public IActionResult Clock()
        {
            return View();
        }
    }
}
