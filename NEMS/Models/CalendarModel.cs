using NEMS.Data;

namespace NEMS.Models
{
    public class CalendarModel
    {
        public IEnumerable<ApplicationUser>? Users { get; set; }
        public string? User { get; set; }
        public List<string>? Participants { get; set; }
        public string? Events { get; set; }
        public Event? Event { get; set; }
    }
}
