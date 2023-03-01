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
        private readonly UserManager<ApplicationUser> _userManager;
        public ApplicationUser user { get; private set; }

        public ClockController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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
                    time.clockout = now;
                    time.rClockout = Rounder.floorTime(time.clockout);
                    TimeSpan diff = time.clockout - time.clockin;
                    TimeSpan rDiff = time.rClockout - Rounder.setStartOT(time.rClockin);
                    time.worktime = diff.TotalHours;
                    if(rDiff.TotalHours >= 10)
                    {
                        time.ot = rDiff.TotalHours - 9;
                        time.et = 0;
                    }else if(time.worktime < 9)
                    {
                        time.et = 9 - time.worktime;
                        time.ot = 0;
                    }

                    _db.TimeTables.Update(time);
                    _db.SaveChanges();
                }
            }
            else
            {
                if (HttpContext.Request.Form["register"] == "clockin")
                {
                    time.uid = HomeController._user.Id;
                    time.date = now;
                    time.clockin = Rounder.setStart(now);
                    time.rClockin = Rounder.ceilingTime(time.clockin);

                    _db.TimeTables.Add(time);
                    _db.SaveChanges();
                }
                else if (HttpContext.Request.Form["register"] == "clockout")
                {
                    time.uid = HomeController._user.Id;
                    time.date = now;
                    time.clockout = now;
                    time.rClockout = Rounder.floorTime(time.clockout);

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

            time.uid = HomeController._user.Id;
            time.date = time.clockin;
            time.clockin = Rounder.setStart(time.clockin);
            time.rClockin = Rounder.ceilingTime(time.clockin);
            time.rClockout = Rounder.floorTime(time.clockout);

            TimeSpan diff = time.clockout - time.clockin;
            TimeSpan rDiff = time.rClockout - Rounder.setStartOT(time.rClockin);
            time.worktime = diff.TotalHours;
            if (rDiff.TotalHours >= 10)
            {
                time.ot = rDiff.TotalHours - 9;
                time.et = 0;
            }
            else if (time.worktime < 9)
            {
                time.et = 9 - time.worktime;
                time.ot = 0;
            }

            _db.TimeTables.Update(time);
            _db.SaveChanges();

            return RedirectToAction("Clock");
        }
    }
}
