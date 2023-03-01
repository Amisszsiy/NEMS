using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NEMS.Data;
using NEMS.Models;
using System.Diagnostics;

namespace NEMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public static ApplicationUser _user { get; private set; }

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;

        }

        public IActionResult Index()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            if (currentUser != null)
            {
                _user = _db.Users.Find(_userManager.GetUserId(currentUser));
            }
            if (_user != null)
            {
                ViewData["userFirstName"] = _user.FirstName;
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}