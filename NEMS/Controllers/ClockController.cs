﻿using Microsoft.AspNetCore.Authorization;
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
                if(HttpContext.Request.Form["register"] == "clockout")//get that record and update
                {
                    time = _db.TimeTables.FirstOrDefault(x => x.date.Date == now.Date && x.uid == _userService.getCurrentUser().Id);
                    if(time.clockin.Ticks != 0)
                    {
                        time = _clock.calculateClockOut(time, now);
                    }
                    else
                    {
                        time = _clock.clockOut(time, now);
                    }
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

        [Authorize(Roles = "Admin")]
        public IActionResult AddPersonWorkTime()
        {
            PersonWorkTime workTime = new PersonWorkTime();
            workTime.Users = _db.Users.Where(x => x.Id != _userService.getCurrentUser().Id);
            workTime.User = workTime.Users.First().FirstName;
            return View(workTime);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPersonWorkTime(PersonWorkTime workTime)
        {
            workTime.TimeTable.uid = workTime.User;

            if(!_db.TimeTables.Any(x => x.date == workTime.TimeTable.clockin.Date && x.uid == workTime.TimeTable.uid))
            {
                workTime.TimeTable = _clock.addWorkDay(workTime.TimeTable);
                _db.TimeTables.Add(workTime.TimeTable);
                _db.SaveChanges();
            }

            return Redirect("../Home/Index");
        }

        [Authorize(Roles = "Sales")]
        public IActionResult AddTime()
        {
            return View();
        }

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
            TimeTable? today = _db.TimeTables.FirstOrDefault(x => x.date.Date == DateTime.Now.Date && x.uid == _userService.getCurrentUser().Id);

            if (today != null)
            {
                todayClock.clockIn = (today.clockin.Ticks == 0) ? null : today.clockin.ToShortTimeString();
                todayClock.clockOut = (today.clockout.Ticks == 0) ? null : today.clockout.ToShortTimeString();
            }

            IEnumerable<TimeTable> thisMonth = _db.TimeTables.Where(x => x.date.Month == DateTime.Now.Month)
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
