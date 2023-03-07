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
}
