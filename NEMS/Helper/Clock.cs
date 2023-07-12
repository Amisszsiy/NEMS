using NEMS.Controllers;
using NEMS.Data;
using NEMS.Models;

namespace NEMS.Helper
{
    public interface IClockService
    {
        TimeTable clockIn(TimeTable time, DateTime now);
        TimeTable clockOut(TimeTable time, DateTime now);
        TimeTable calculateClockOut(TimeTable time, DateTime now);
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
            time.uid = _userService.getCurrentUser().Id;
            time.date = now.Date;
            time.clockout = now;
            time.rClockout = Rounder.floorTime(time.clockout);

            return time;
        }
        public TimeTable calculateClockOut(TimeTable time, DateTime now)
        {
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

            return time;
        }

        public TimeTable addWorkDay(TimeTable time)
        {
            time.date = time.clockin.Date;
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

            return time;
        }

        public TimeTable editWorkDay(TimeTable editTime, TimeTable time)
        {
            editTime.date = time.clockin.Date;
            editTime.clockin = Rounder.setStart(time.clockin);
            editTime.rClockin = Rounder.ceilingTime(editTime.clockin);
            editTime.clockout = time.clockout;
            editTime.rClockout = Rounder.floorTime(editTime.clockout);

            TimeSpan diff = editTime.clockout - editTime.clockin;
            TimeSpan rDiff = editTime.rClockout - Rounder.setStartOT(editTime.rClockin);
            editTime.worktime = diff.TotalHours;
            if (rDiff.TotalHours >= 10)
            {
                editTime.ot = rDiff.TotalHours - 9;
                editTime.et = 0;
            }
            else if (editTime.worktime < 9)
            {
                editTime.et = 9 - editTime.worktime;
                editTime.ot = 0;
            }
            else
            {
                editTime.et = 0;
                editTime.ot = 0;
            }

            return editTime;
        }
    }
}
