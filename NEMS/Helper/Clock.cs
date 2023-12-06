using NEMS.Controllers;
using NEMS.Data;
using NEMS.Models;

namespace NEMS.Helper
{
    public interface IClockService
    {
        TimeTable clockIn(TimeTable time, DateTime now);
        TimeTable clockOut(TimeTable time, DateTime now);
        TimeTable addWorkDay(TimeTable time);
        TimeTable editWorkDay(TimeTable editTime, TimeTable time);
    }
    public class Clock : IClockService
    {
        private readonly IUserService _userService;

        public Clock(IUserService userService)
        {
            _userService = userService;
        }
        public TimeTable clockIn(TimeTable time, DateTime now)
        {
            time.uid = _userService.getCurrentUser().Id;
            time.date = now.Date;
            time.clockin = Rounder.setStart(now);
            time.rClockin = Rounder.ceilingTime(time.clockin);

            return time;
        }
        public TimeTable clockOut(TimeTable time, DateTime now)
        {
            //Check if already clock-in
            if(time.clockin.Ticks == 0)
            {
                time.uid = _userService.getCurrentUser().Id;
                time.date = now.Date;
            }

            time.clockout = now;
            time.rClockout = Rounder.floorTime(time.clockout);

            if (time.clockin.Ticks != 0)
            {
                return calculateWorkTime(time);
            }

            return time;
        }

        public TimeTable addWorkDay(TimeTable time)
        {
            time.date = time.clockin.Date;
            time.clockin = Rounder.setStart(time.clockin);
            time.rClockin = Rounder.ceilingTime(time.clockin);
            time.rClockout = Rounder.floorTime(time.clockout);

            if(time.clockin.Ticks != 0 && time.clockout.Ticks != 0)
            {
                return calculateWorkTime(time);
            }

            return time;
        }

        public TimeTable editWorkDay(TimeTable editTime, TimeTable time)
        {
            editTime.date = time.clockin.Date;
            editTime.clockin = Rounder.setStart(time.clockin);
            editTime.rClockin = Rounder.ceilingTime(editTime.clockin);
            editTime.clockout = time.clockout;
            editTime.rClockout = Rounder.floorTime(editTime.clockout);

            return calculateWorkTime(editTime);
        }

        private TimeTable calculateWorkTime(TimeTable worktime)
        {
            TimeSpan diff = worktime.clockout - worktime.clockin;
            TimeSpan rDiff = worktime.rClockout - Rounder.setStartOT(worktime.rClockin);
            worktime.worktime = diff.TotalHours;
            if (rDiff.TotalHours >= 10)
            {
                worktime.ot = rDiff.TotalHours - 9;
                worktime.et = 0;
            }
            else if (worktime.worktime < 9)
            {
                worktime.et = 9 - worktime.worktime;
                worktime.ot = 0;
            }
            else
            {
                worktime.et = 0;
                worktime.ot = 0;
            }
            return worktime;
        }
    }
}
