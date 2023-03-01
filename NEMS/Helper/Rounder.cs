namespace NEMS.Helper
{
    public static class Rounder
    {
        static TimeSpan round = TimeSpan.FromMinutes(30);

        public static DateTime ceilingTime(DateTime time)
        {
            long ticks = ((time.Ticks + round.Ticks - 1) / round.Ticks) * round.Ticks;

            return new DateTime(ticks);
        }

        public static DateTime floorTime(DateTime time)
        {
            long ticks = (time.Ticks / round.Ticks) * round.Ticks;

            return new DateTime(ticks);
        }

        public static DateTime setStart(DateTime time)
        {
            TimeSpan sevenHalf = new TimeSpan(7, 30, 00);
            DateTime earliest = time.Date + sevenHalf;
            if(time<earliest)
            {
                time = earliest;
            }
            return time;
        }

        public static DateTime setStartOT(DateTime time)
        {
            TimeSpan eightHalf = new TimeSpan(8, 30, 00);
            DateTime earliest = time.Date + eightHalf;
            if (time < earliest)
            {
                time = earliest;
            }
            return time;
        }
    }
}
