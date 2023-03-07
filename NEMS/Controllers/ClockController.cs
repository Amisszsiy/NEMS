using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NEMS.Data;
using NEMS.Helper;
using NEMS.Models;
using System.Linq;

namespace NEMS.Controllers
{
    public class ClockController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IClockService _clock;
        public ApplicationUser user { get; private set; }

        public ClockController(ApplicationDbContext db, IClockService clock)
        {
            _db = db;
            _clock = clock;
        }
        public IActionResult Clock()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(TimeTable time)
        {
            DateTime now = DateTime.Now;
            if (_db.TimeTables.Any(x => x.date.Date == now.Date && x.uid == HomeController._user.Id))
            {
                if (HttpContext.Request.Form["register"] == "clockin")
                {
                    return RedirectToAction("Clock");
                }
                else if(HttpContext.Request.Form["register"] == "clockout")//get that record and update
                {
                    time = _db.TimeTables.FirstOrDefault(x => x.date.Date == now.Date && x.uid == HomeController._user.Id);
                    time = _clock.calculateClockOut(time, now);
                    _db.TimeTables.Update(time);
                    _db.SaveChanges();
                }
            }
            else
            {
                if (HttpContext.Request.Form["register"] == "clockin")
                {
                    time = _clock.clockIn(time, now);

                    _db.TimeTables.Add(time);
                    _db.SaveChanges();
                }
                else if (HttpContext.Request.Form["register"] == "clockout")
                {
                    time = _clock.clockOut(time, now);

                    _db.TimeTables.Add(time);
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("Clock");
        }

        //Test input
        public IActionResult TestInput()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TestInput(TimeTable time)
        {
            if (time == null)
            {
                return RedirectToAction("Clock");
            }

            time = _clock.addWorkDay(time);

            _db.TimeTables.Update(time);
            _db.SaveChanges();

            return RedirectToAction("Clock");
        }
    }
}
