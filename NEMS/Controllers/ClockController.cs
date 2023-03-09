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
        private readonly IUserService _userService;
        public ApplicationUser user { get; private set; }

        public ClockController(ApplicationDbContext db, IClockService clock, IUserService userService)
        {
            _db = db;
            _clock = clock;
            _userService = userService;
        }
        public IActionResult Clock()
        {
            ClockViewModel todayClock= new ClockViewModel();
            todayClock = getClockInfo(todayClock);
            return View(todayClock);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(TimeTable? time)
        {
            DateTime now = DateTime.Now;
            if (_db.TimeTables.Any(x => x.date.Date == now.Date && x.uid == _userService.getCurrentUser().Id))
            {
                if (HttpContext.Request.Form["register"] == "clockin")
                {
                    return RedirectToAction("Clock");
                }
                else if(HttpContext.Request.Form["register"] == "clockout")//get that record and update
                {
                    time = _db.TimeTables.FirstOrDefault(x => x.date.Date == now.Date && x.uid == _userService.getCurrentUser().Id);
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
        public IActionResult AddTime()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTime(TimeTable time)
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

        private ClockViewModel getClockInfo(ClockViewModel todayClock)
        {
            TimeTable? today = _db.TimeTables.FirstOrDefault(x => x.date.Date == DateTime.Now.Date && x.uid == _userService.getCurrentUser().Id);
            if (today == null)
            {
                todayClock.clockIn = null;
                todayClock.clockOut = null;
            }
            else if (today.clockout < today.clockin)
            {
                todayClock.clockIn = today.clockin.ToShortTimeString();
                todayClock.clockOut = null;
            }
            else if (today.worktime != 0)
            {
                todayClock.clockIn = today.clockin.ToShortTimeString();
                todayClock.clockOut = today.clockout.ToShortTimeString();
            }
            else
            {
                todayClock.clockIn = null;
                todayClock.clockOut = today.clockout.ToShortTimeString();
            }
            IEnumerable<TimeTable> thisMonth = _db.TimeTables.Where(x => x.date.Month == DateTime.Now.Month)
                .Where(x => x.uid == _userService.getCurrentUser().Id);
            double thisMonthOT = thisMonth.Sum(x => x.ot);
            double thisMonthET = thisMonth.Sum(x => x.et);
            todayClock.thisMonthOT = thisMonthOT;
            todayClock.thisMonthET = thisMonthET;
            todayClock.thisMonthPayOT = thisMonthOT - thisMonthET;

            return todayClock;
        }
    }
}
