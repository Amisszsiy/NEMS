using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NEMS.Data;
using NEMS.Helper;
using NEMS.Models;
using System.Linq;

namespace NEMS.Controllers
{
    [Authorize]
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
            //Get today clockin/out time and ot/et summary of current month
            todayClock = getClockInfo(todayClock);

            return View(todayClock);
        }

        //Operate clock in/out depending on conditions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(TimeTable? time)
        {
            DateTime now = DateTime.Now;
            //Search if user have been clocked today
            //If there is a today record
            if (_db.TimeTables.Any(x => x.date.Date == now.Date && x.uid == _userService.getCurrentUser().Id))
            {
                if(HttpContext.Request.Form["register"] == "clockout")//get that record and update
                {
                    time = _db.TimeTables.FirstOrDefault(x => x.date.Date == now.Date && x.uid == _userService.getCurrentUser().Id);
                    time = _clock.clockOut(time, now);

                    _db.TimeTables.Update(time);
                    _db.SaveChanges();
                }
            }
            else
            {
                //If no today record, it can be assumed that today hasn't clocked in yet
                if (HttpContext.Request.Form["register"] == "clockin")
                {
                    //Create record for today and put only clock in time
                    time = _clock.clockIn(time, now);

                    _db.TimeTables.Add(time);
                    _db.SaveChanges();
                }
                else if (HttpContext.Request.Form["register"] == "clockout")
                {
                    //If today hasn't been clocked in yet, put only clock out time stamp
                    time = _clock.clockOut(time, now);

                    _db.TimeTables.Add(time);
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("Clock");
        }

        //Return specific user worktime adding page
        [Authorize(Roles = "Admin")]
        public IActionResult AddPersonWorkTime()
        {
            PersonWorkTime workTime = new PersonWorkTime();
            //Get list of users
            workTime.Users = _db.Users.Where(x => x.Id != _userService.getCurrentUser().Id);
            //Set default selected user
            workTime.User = workTime.Users.First().FirstName;

            return View(workTime);
        }

        //Add workday for specific user by admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPersonWorkTime(PersonWorkTime workTime)
        {
            workTime.TimeTable.uid = workTime.User;
            //Excluding admin user from user selecting list
            workTime.Users = _db.Users.Where(x => x.Id != _userService.getCurrentUser().Id);

            //Check if workday of specific user exist
            if (!_db.TimeTables.Any(x => x.date == workTime.TimeTable.clockin.Date && x.uid == workTime.TimeTable.uid))
            {
                workTime.TimeTable = _clock.addWorkDay(workTime.TimeTable);
                _db.TimeTables.Add(workTime.TimeTable);
                _db.SaveChanges();
            }

            return View("AddPersonWorkTime", workTime);
        }

        //Return workday adding page
        [Authorize(Roles = "Sales")]
        public IActionResult AddTime()
        {
            return View();
        }

        //Adding workday by sales user
        [Authorize(Roles = "Sales")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTime(TimeTable time)
        {
            if (time == null)
            {
                return RedirectToAction("Clock");
            }

            time.uid = _userService.getCurrentUser().Id;

            if(!_db.TimeTables.Any(x => x.date == time.clockin.Date && x.uid == time.uid))
            {
                time = _clock.addWorkDay(time);

                _db.TimeTables.Update(time);
                _db.SaveChanges();
            }

            return RedirectToAction("Clock");
        }

        private ClockViewModel getClockInfo(ClockViewModel todayClock)
        {
            DateTime now = DateTime.Today;
            DateTime offset;
            //Fetch today user's clockin/out data
            TimeTable? today = _db.TimeTables.FirstOrDefault(x => x.date.Date == DateTime.Now.Date && x.uid == _userService.getCurrentUser().Id);

            //If no clockin/out data aka Ticks = 0, set model to null to make it invisible at frontend
            if (today != null)
            {
                todayClock.clockIn = (today.clockin.Ticks == 0) ? null : today.clockin.ToShortTimeString();
                todayClock.clockOut = (today.clockout.Ticks == 0) ? null : today.clockout.ToShortTimeString();
            }

            //Set offset to 25th of current month or previous month depending on current date
            //Calculate ot/et is range from 26 of previous month to 25 of current month
            if(now.Month == 1)
            {
                offset = (now.Day > 25) ? new DateTime(now.Year, now.Month, 26) : new DateTime(now.Year-1, 12, 26);
            }
            else
            {
                offset = (now.Day > 25) ? new DateTime(now.Year, now.Month, 26) : new DateTime(now.Year, now.Month - 1, 26);
            }

            //Query total worktime of current month based on OT offset above
            IEnumerable<TimeTable> thisMonth = _db.TimeTables.Where(x => x.date.Date >= offset.Date)
                .Where(x => x.uid == _userService.getCurrentUser().Id)
                .Where(x => x.worktime != 0);
            double thisMonthOT = thisMonth.Sum(x => x.ot);
            double thisMonthET = thisMonth.Sum(x => x.et);
            todayClock.thisMonthOT = thisMonthOT;
            todayClock.thisMonthET = thisMonthET;
            todayClock.thisMonthPayOT = thisMonthOT - thisMonthET;

            return todayClock;
        }
    }
}
